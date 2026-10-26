using FinanceAgent.Agents;
using FinanceAgent.Data;
using FinanceAgent.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceAgent.Services;

// Write-side operations on transactions that come from the user
public class TransactionService
{
    private readonly UserDbContextFactory _dbFactory;

    public TransactionService(UserDbContextFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    // Changes a transaction's category. With learnRule, the merchant becomes a keyword rule so
    // future imports from the same merchant are categorized automatically.
    public async Task<int> RecategorizeAsync(int transactionId, int categoryId, bool learnRule, string actor)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var txn = await db.Transactions.Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == transactionId)
            ?? throw new InvalidOperationException($"Transaction {transactionId} not found.");
        var category = await db.Categories.FindAsync(categoryId)
            ?? throw new InvalidOperationException($"Category {categoryId} not found.");

        var oldName = txn.Category?.Name ?? DbSeeder.Uncategorized;
        txn.CategoryId = category.Id;
        txn.CategorizationConfidence = 1.0;

        db.AgentActions.Add(new AgentAction
        {
            AgentName = actor,
            ActionType = "Recategorized",
            Input = txn.RawDescription,
            Output = category.Name,
            Reasoning = $"Changed from {oldName} to {category.Name}"
        });

        int alsoUpdated = 0;
        var keyword = txn.Merchant.Trim();
        if (learnRule && keyword.Length >= 3 && category.Name != DbSeeder.Uncategorized)
        {
            var rule = await db.CategoryRules.FirstOrDefaultAsync(r => r.Keyword == keyword);
            if (rule is null) db.CategoryRules.Add(new CategoryRule { Keyword = keyword, CategoryId = category.Id });
            else rule.CategoryId = category.Id;

            // Apply the correction to other uncategorized rows from the same merchant right away
            var siblings = await db.Transactions
                .Where(t => t.Id != txn.Id && t.Merchant == txn.Merchant &&
                            (t.CategoryId == null || t.Category!.Name == DbSeeder.Uncategorized))
                .ToListAsync();
            foreach (var s in siblings)
            {
                s.CategoryId = category.Id;
                s.CategorizationConfidence = 1.0;
            }
            alsoUpdated = siblings.Count;
        }

        await db.SaveChangesAsync();
        return alsoUpdated;
    }

    public async Task DismissAnomalyAsync(int transactionId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var txn = await db.Transactions.FindAsync(transactionId)
            ?? throw new InvalidOperationException($"Transaction {transactionId} not found.");

        db.AgentActions.Add(new AgentAction
        {
            AgentName = "User",
            ActionType = "DismissedAnomaly",
            Input = txn.RawDescription,
            Output = "Dismissed",
            Reasoning = txn.AnomalyReason ?? string.Empty
        });

        txn.FlaggedAsAnomaly = false;
        txn.AnomalyDismissed = true;
        await db.SaveChangesAsync();
    }
}
