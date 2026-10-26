using FinanceAgent.Data;
using FinanceAgent.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceAgent.Agents;

// Deterministic checks so every flag comes with a plain-language reason.
public class AnomalyDetectionAgent
{
    private const decimal UnusualMultiplier = 2.0m;    // charge vs. merchant's typical charge
    private const decimal UnusualMinDifference = 20m;  // ignore $3 coffee -> $7 coffee
    private const decimal LargeNewMerchant = 300m;
    private const int DuplicateWindowDays = 3;

    private readonly UserDbContextFactory _dbFactory;

    public AnomalyDetectionAgent(UserDbContextFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<int> ScanAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var all = await db.Transactions.Include(t => t.Category).OrderBy(t => t.Date).ThenBy(t => t.Id).ToListAsync();
        var outflows = all.Where(t => t.Amount < 0 && t.Category?.ExcludeFromBudget != true).ToList();
        var byMerchant = outflows.GroupBy(t => MerchantKey(t)).ToDictionary(g => g.Key, g => g.ToList());

        int newlyFlagged = 0;

        foreach (var txn in all)
        {
            if (txn.AnomalyDismissed) continue;

            var reason = txn.Amount < 0 && txn.Category?.ExcludeFromBudget != true
                ? FindReason(txn, byMerchant[MerchantKey(txn)])
                : null;

            if (reason is not null && !txn.FlaggedAsAnomaly)
            {
                newlyFlagged++;
                db.AgentActions.Add(new AgentAction
                {
                    AgentName = nameof(AnomalyDetectionAgent),
                    ActionType = "FlaggedAnomaly",
                    Input = $"{txn.Date:yyyy-MM-dd} {txn.RawDescription} {txn.Amount:C}",
                    Output = "Flagged",
                    Reasoning = reason
                });
            }

            txn.FlaggedAsAnomaly = reason is not null;
            txn.AnomalyReason = reason;
        }

        await db.SaveChangesAsync();
        return newlyFlagged;
    }

    public static string MerchantKey(Transaction t) =>
        (string.IsNullOrWhiteSpace(t.Merchant) ? t.RawDescription : t.Merchant).Trim().ToUpperInvariant();

    private static string? FindReason(Transaction txn, List<Transaction> sameMerchant)
    {
        var amount = -txn.Amount;
        var others = sameMerchant.Where(o => o.Id != txn.Id).ToList();

        // 1. Possible double charge: same merchant, same amount, within a few days, and this is the later one
        var twin = others.FirstOrDefault(o =>
            o.Amount == txn.Amount &&
            (o.Date < txn.Date || (o.Date == txn.Date && o.Id < txn.Id)) &&
            (txn.Date - o.Date).TotalDays <= DuplicateWindowDays);
        if (twin is not null)
            return $"Possible duplicate charge: same amount ({amount:C}) at this merchant on {twin.Date:MMM d}.";

        // 2. Subscription price change compared to the previous charge
        if (txn.Category?.Name == "Subscriptions")
        {
            var previous = others.Where(o => o.Date < txn.Date).OrderByDescending(o => o.Date).FirstOrDefault();
            if (previous is not null && previous.Amount != txn.Amount)
            {
                var prevAmount = -previous.Amount;
                return amount > prevAmount
                    ? $"Subscription price went up from {prevAmount:C} to {amount:C}."
                    : $"Subscription charge changed from {prevAmount:C} to {amount:C}.";
            }
        }

        // 3. Much bigger than this merchant's usual charge
        if (others.Count >= 2)
        {
            var typical = Median(others.Select(o => -o.Amount));
            if (amount > typical * UnusualMultiplier && amount - typical >= UnusualMinDifference)
                return $"{amount:C} is {amount / typical:0.0}x your typical {typical:C} at this merchant.";
        }

        // 4. Large charge at a merchant you've never used before
        if (others.Count == 0 && amount >= LargeNewMerchant)
            return $"Large first-time charge of {amount:C} at a new merchant.";

        return null;
    }

    private static decimal Median(IEnumerable<decimal> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        var mid = sorted.Count / 2;
        return sorted.Count % 2 == 1 ? sorted[mid] : (sorted[mid - 1] + sorted[mid]) / 2;
    }
}
