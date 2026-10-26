using System.Text.RegularExpressions;
using FinanceAgent.Data;
using FinanceAgent.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceAgent.Agents;

public class RuleBasedCategorizer
{
    private readonly UserDbContextFactory _dbFactory;

    // Keyword -> Category name. Checked against the description as whole words, case-insensitive.
    // Order matters: first match wins. Rules learned from user corrections are checked first.
    private static readonly List<(string Keyword, string CategoryName)> Rules = new()
    {
        ("STARBUCKS", "Dining"),
        ("MCDONALD", "Dining"),
        ("CHIPOTLE", "Dining"),
        ("RESTAURANT", "Dining"),
        ("DOORDASH", "Dining"),
        ("UBER EATS", "Dining"),

        ("WHOLE FOODS", "Groceries"),
        ("KROGER", "Groceries"),
        ("H-E-B", "Groceries"),
        ("HEB", "Groceries"),
        ("TRADER JOE", "Groceries"),
        ("ALDI", "Groceries"),

        ("SHELL", "Gas"),
        ("CHEVRON", "Gas"),
        ("EXXON", "Gas"),
        ("VALERO", "Gas"),

        ("UBER", "Transportation"),
        ("LYFT", "Transportation"),
        ("PARKING", "Transportation"),

        ("NETFLIX", "Subscriptions"),
        ("SPOTIFY", "Subscriptions"),
        ("HULU", "Subscriptions"),
        ("DISNEY PLUS", "Subscriptions"),

        ("PLANET FITNESS", "Health & Fitness"),
        ("CVS", "Health & Fitness"),
        ("WALGREENS", "Health & Fitness"),

        ("ELECTRIC", "Utilities"),
        ("COMCAST", "Utilities"),
        ("AT&T", "Utilities"),

        ("AMAZON", "Shopping"),
        ("AMZN", "Shopping"),
        ("TARGET", "Shopping"),
        ("WALMART", "Shopping"),

        ("PAYROLL", "Income"),
        ("DIRECT DEPOSIT", "Income"),

        ("VENMO", "Transfers"),
        ("ZELLE", "Transfers"),
        ("TRANSFER", "Transfers"),
    };

    public RuleBasedCategorizer(UserDbContextFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    // Whole-word match so "SHELL" doesn't catch "SHELLFISH SHACK". A trailing S is allowed
    // so "MCDONALD" still matches "MCDONALDS".
    public static bool Matches(string description, string keyword) =>
        Regex.IsMatch(description, $@"(?<![A-Za-z0-9]){Regex.Escape(keyword)}S?(?![A-Za-z0-9])", RegexOptions.IgnoreCase);

    public async Task<int> CategorizeUncategorizedAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var categories = await db.Categories.ToListAsync();
        var fallback = categories.First(c => c.Name == DbSeeder.Uncategorized);

        var uncategorized = await db.Transactions
            .Where(t => t.CategoryId == null || t.CategoryId == fallback.Id)
            .ToListAsync();

        if (uncategorized.Count == 0) return 0;

        var learned = (await db.CategoryRules.OrderByDescending(r => r.CreatedAt).ToListAsync())
            .Select(r => (r.Keyword, CategoryName: categories.First(c => c.Id == r.CategoryId).Name, Learned: true));
        // Built-in rules only apply while their category exists, since users can rename or delete categories
        var allRules = learned.Concat(Rules.Select(r => (r.Keyword, r.CategoryName, Learned: false)))
            .Where(r => categories.Any(c => c.Name == r.CategoryName))
            .ToList();

        int matchedCount = 0;

        foreach (var txn in uncategorized)
        {
            var match = allRules.FirstOrDefault(r => Matches(txn.RawDescription, r.Keyword));
            var category = match.CategoryName is null ? null : categories.FirstOrDefault(c => c.Name == match.CategoryName);

            if (category is null)
            {
                // Only log the first time a row lands in Uncategorized, not on every re-run
                if (txn.CategoryId is null)
                {
                    txn.CategoryId = fallback.Id;
                    txn.CategorizationConfidence = 0.0;
                    db.AgentActions.Add(new AgentAction
                    {
                        AgentName = nameof(RuleBasedCategorizer),
                        ActionType = "LeftForReview",
                        Input = txn.RawDescription,
                        Output = DbSeeder.Uncategorized,
                        Reasoning = "No keyword match — defaulted to Uncategorized"
                    });
                }
                continue;
            }

            txn.CategoryId = category.Id;
            txn.CategorizationConfidence = 1.0;
            matchedCount++;

            // Log it — this is what makes the agent's decisions demoable later
            db.AgentActions.Add(new AgentAction
            {
                AgentName = nameof(RuleBasedCategorizer),
                ActionType = "Categorized",
                Input = txn.RawDescription,
                Output = category.Name,
                Reasoning = match.Learned
                    ? $"Matched rule learned from your correction: '{match.Keyword}'"
                    : $"Matched keyword '{match.Keyword}'"
            });
        }

        await db.SaveChangesAsync();
        return matchedCount;
    }
}
