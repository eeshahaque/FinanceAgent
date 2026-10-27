using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using FinanceAgent.Data;
using FinanceAgent.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceAgent.Services;

public record ImportResult(int Imported, int SkippedDuplicates, int SkippedInvalid);

public class TransactionImportService
{
    private readonly UserDbContextFactory _dbFactory;

    // Header names used by common bank exports (Chase, BofA, Capital One, Wells Fargo, Mint, etc.)
    private static readonly string[] DateHeaders = { "Date", "Transaction Date", "Posting Date", "Posted Date", "Trans. Date" };
    private static readonly string[] DescriptionHeaders = { "Description", "Payee", "Merchant", "Name", "Memo", "Details" };
    private static readonly string[] AmountHeaders = { "Amount", "Transaction Amount" };
    private static readonly string[] DebitHeaders = { "Debit", "Withdrawal", "Withdrawals" };
    private static readonly string[] CreditHeaders = { "Credit", "Deposit", "Deposits" };

    private static readonly string[] DateFormats =
        { "M/d/yyyy", "MM/dd/yyyy", "M/d/yy", "yyyy-MM-dd", "yyyy/MM/dd", "MM-dd-yyyy", "d-MMM-yyyy", "MMM d, yyyy" };

    public TransactionImportService(UserDbContextFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public static List<CsvTransactionRow> ParseCsv(Stream csvStream, out int invalidRows)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            BadDataFound = null,
            TrimOptions = TrimOptions.Trim
        };

        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, config);

        if (!csv.Read() || !csv.ReadHeader())
            throw new InvalidOperationException("The file is empty.");
        var headers = csv.HeaderRecord ?? Array.Empty<string>();

        // Names are tried in priority order, so Chase's leading "Details" column (DEBIT/CREDIT)
        // loses to its "Description" column
        int Find(string[] names) => names
            .Select(n => Array.FindIndex(headers, h => n.Equals(h.Trim(), StringComparison.OrdinalIgnoreCase)))
            .FirstOrDefault(i => i >= 0, -1);

        var dateCol = Find(DateHeaders);
        var descCol = Find(DescriptionHeaders);
        var amountCol = Find(AmountHeaders);
        var debitCol = Find(DebitHeaders);
        var creditCol = Find(CreditHeaders);

        if (dateCol < 0 || descCol < 0 || (amountCol < 0 && debitCol < 0 && creditCol < 0))
        {
            throw new InvalidOperationException(
                "Couldn't find the columns this importer needs. Expected a date column (e.g. 'Date'), " +
                "a description column (e.g. 'Description'), and either 'Amount' or 'Debit'/'Credit'. " +
                $"Found: {string.Join(", ", headers)}");
        }

        var rows = new List<CsvTransactionRow>();
        invalidRows = 0;

        while (csv.Read())
        {
            var description = csv.GetField(descCol)?.Trim() ?? string.Empty;

            if (!TryParseDate(csv.GetField(dateCol), out var date) || description.Length == 0)
            {
                invalidRows++;
                continue;
            }

            decimal? amount;
            if (amountCol >= 0)
            {
                amount = ParseMoney(csv.GetField(amountCol));
            }
            else
            {
                // Separate debit/credit columns: debits are money out, so they become negative
                var debit = debitCol < 0 ? null : ParseMoney(csv.GetField(debitCol));
                var credit = creditCol < 0 ? null : ParseMoney(csv.GetField(creditCol));
                amount = debit is null && credit is null ? null : (credit ?? 0) - Math.Abs(debit ?? 0);
            }

            if (amount is null)
            {
                invalidRows++;
                continue;
            }

            rows.Add(new CsvTransactionRow(date, description, amount.Value));
        }

        return rows;
    }

    public async Task<ImportResult> ImportFromCsvAsync(Stream csvStream, int accountId)
    {
        var rows = ParseCsv(csvStream, out var invalid);

        await using var db = await _dbFactory.CreateDbContextAsync();
        var account = await db.Accounts.FindAsync(accountId)
            ?? throw new InvalidOperationException($"Account {accountId} not found.");

        if (rows.Count == 0) return new ImportResult(0, 0, invalid);

        // Duplicate detection: re-importing an overlapping statement shouldn't double-count.
        // Counts are compared rather than a plain set so two genuine identical coffees on the
        // same day in one file are both kept.
        var minDate = rows.Min(r => r.Date).Date;
        var maxDate = rows.Max(r => r.Date).Date.AddDays(1);
        var existingCounts = (await db.Transactions
                .Where(t => t.AccountId == accountId && t.Date >= minDate && t.Date < maxDate)
                .Select(t => new { t.Date, t.Amount, t.RawDescription })
                .ToListAsync())
            .GroupBy(t => DedupeKey(t.Date, t.Amount, t.RawDescription))
            .ToDictionary(g => g.Key, g => g.Count());

        var newTransactions = new List<Transaction>();
        int duplicates = 0;

        foreach (var row in rows)
        {
            var key = DedupeKey(row.Date, row.Amount, row.Description);
            if (existingCounts.TryGetValue(key, out var remaining) && remaining > 0)
            {
                existingCounts[key] = remaining - 1;
                duplicates++;
                continue;
            }

            newTransactions.Add(new Transaction
            {
                AccountId = account.Id,
                Date = row.Date.Date,
                Merchant = ExtractMerchantGuess(row.Description),
                Amount = row.Amount,
                RawDescription = row.Description
            });
        }

        db.Transactions.AddRange(newTransactions);
        await db.SaveChangesAsync();

        return new ImportResult(newTransactions.Count, duplicates, invalid);
    }

    // Removes rows that were imported more than once before duplicate detection existed.
    // Keeps the earliest copy of each (account, date, amount, description) group.
    public async Task<int> RemoveDuplicateImportsAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var all = await db.Transactions.OrderBy(t => t.Id).ToListAsync();

        var extras = all
            .GroupBy(t => (t.AccountId, Key: DedupeKey(t.Date, t.Amount, t.RawDescription)))
            .SelectMany(g => g.Skip(1))
            .ToList();

        if (extras.Count == 0) return 0;

        db.Transactions.RemoveRange(extras);
        await db.SaveChangesAsync();
        return extras.Count;
    }

    private static string DedupeKey(DateTime date, decimal amount, string description) =>
        $"{date:yyyy-MM-dd}|{amount:0.00}|{description.Trim().ToUpperInvariant()}";

    private static bool TryParseDate(string? text, out DateTime date)
    {
        date = default;
        if (string.IsNullOrWhiteSpace(text)) return false;
        return DateTime.TryParseExact(text.Trim(), DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date)
            || DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
    }

    private static decimal? ParseMoney(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var cleaned = text.Trim().Replace("$", "").Replace(",", "");
        // Accounting style: (12.34) means -12.34
        var negative = cleaned.StartsWith('(') && cleaned.EndsWith(')');
        cleaned = cleaned.Trim('(', ')');
        if (!decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)) return null;
        return negative ? -value : value;
    }

    // Naive first pass — bank descriptions are messy (store numbers, locations), so this
    // drops "#1234"-style tokens and keeps the first few words as a rough merchant name.
    public static string ExtractMerchantGuess(string rawDescription)
    {
        var words = rawDescription.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => !w.StartsWith('#') && !w.All(char.IsDigit))
            .Select(StripReferenceCode)
            .ToArray();
        return string.Join(" ", words.Take(Math.Min(3, words.Length))).Trim();
    }

    // Card descriptors often append a per-order code after '*' ("AMAZON.COM*2K4LM0"). Dropping it
    // keeps every order from the same merchant under one name, which the anomaly checks rely on.
    // Codes are told apart from names by containing a digit, so "DOORDASH*MAMAS" stays intact.
    private static string StripReferenceCode(string word)
    {
        var star = word.IndexOf('*');
        if (star <= 0) return word;
        var code = word[(star + 1)..];
        return code.Length >= 4 && code.Any(char.IsDigit) ? word[..star] : word;
    }
}
