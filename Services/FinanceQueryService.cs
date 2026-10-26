using FinanceAgent.Data;
using FinanceAgent.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceAgent.Services;

public record CategorySpend(int CategoryId, string Name, decimal Budget, decimal Spent, bool ExcludeFromBudget)
{
    public decimal Remaining => Budget - Spent;
    public double PercentUsed => Budget > 0 ? (double)(Spent / Budget) : 0;
    public bool OverBudget => Budget > 0 && Spent > Budget;
}

public record MonthSummary(int Year, int Month, decimal Income, decimal Spending, List<CategorySpend> Categories)
{
    public decimal Net => Income - Spending;
    public string Label => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
}

public record TransactionFilter(
    DateTime? From = null,
    DateTime? To = null,
    int? CategoryId = null,
    string? Search = null,
    bool FlaggedOnly = false,
    bool NeedsReviewOnly = false,
    int Limit = 500);

// Read-side queries shared by the pages and agents
public class FinanceQueryService
{
    private readonly UserDbContextFactory _dbFactory;

    public FinanceQueryService(UserDbContextFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<DateTime>> GetMonthsWithDataAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var months = await db.Transactions
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .Select(g => g.Key)
            .ToListAsync();
        return months.Select(m => new DateTime(m.Year, m.Month, 1)).OrderByDescending(d => d).ToList();
    }

    public async Task<MonthSummary> GetMonthSummaryAsync(int year, int month)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);

        var categories = await db.Categories.OrderBy(c => c.Name).ToListAsync();
        var txns = await db.Transactions
            .Where(t => t.Date >= start && t.Date < end)
            .Select(t => new { t.Amount, t.CategoryId })
            .ToListAsync();

        var excluded = categories.Where(c => c.ExcludeFromBudget).Select(c => c.Id).ToHashSet();

        // Spending is net outflow per category, so a refund reduces that category's spend
        var spentByCategory = txns
            .Where(t => t.CategoryId is null || !excluded.Contains(t.CategoryId.Value))
            .GroupBy(t => t.CategoryId ?? categories.First(c => c.Name == DbSeeder.Uncategorized).Id)
            .ToDictionary(g => g.Key, g => Math.Max(0, -g.Sum(t => t.Amount)));

        var incomeIds = categories.Where(c => c.Kind == CategoryKind.Income).Select(c => c.Id).ToHashSet();
        var income = txns.Where(t => t.Amount > 0 && t.CategoryId is int id && incomeIds.Contains(id))
                         .Sum(t => t.Amount);

        var spends = categories
            .Select(c => new CategorySpend(c.Id, c.Name, c.MonthlyBudget,
                spentByCategory.GetValueOrDefault(c.Id), c.ExcludeFromBudget))
            .ToList();

        return new MonthSummary(year, month, income, spends.Sum(s => s.Spent), spends);
    }

    // Net spend per category per month, oldest first. Used by the budget advisor.
    public async Task<Dictionary<int, decimal[]>> GetMonthlySpendByCategoryAsync(IReadOnlyList<DateTime> months)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        if (months.Count == 0) return new();

        var start = months.Min();
        var end = months.Max().AddMonths(1);
        var txns = await db.Transactions
            .Where(t => t.Date >= start && t.Date < end && t.CategoryId != null)
            .Select(t => new { t.Date, t.Amount, CategoryId = t.CategoryId!.Value })
            .ToListAsync();

        return txns
            .GroupBy(t => t.CategoryId)
            .ToDictionary(g => g.Key, g => months
                .Select(m => Math.Max(0, -g.Where(t => t.Date.Year == m.Year && t.Date.Month == m.Month).Sum(t => t.Amount)))
                .ToArray());
    }

    public async Task<List<Transaction>> GetTransactionsAsync(TransactionFilter filter)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var query = db.Transactions.Include(t => t.Category).AsNoTracking().AsQueryable();

        if (filter.From is { } from) query = query.Where(t => t.Date >= from);
        if (filter.To is { } to) query = query.Where(t => t.Date <= to);
        if (filter.CategoryId is { } cid) query = query.Where(t => t.CategoryId == cid);
        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(t => t.RawDescription.Contains(filter.Search) || t.Merchant.Contains(filter.Search));
        if (filter.FlaggedOnly) query = query.Where(t => t.FlaggedAsAnomaly && !t.AnomalyDismissed);
        if (filter.NeedsReviewOnly)
            query = query.Where(t => t.CategoryId == null || t.Category!.Name == DbSeeder.Uncategorized);

        return await query
            .OrderByDescending(t => t.Date).ThenByDescending(t => t.Id)
            .Take(Math.Clamp(filter.Limit, 1, 2000))
            .ToListAsync();
    }

    public async Task<int> CountNeedsReviewAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Transactions.CountAsync(t => t.CategoryId == null || t.Category!.Name == DbSeeder.Uncategorized);
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<List<AgentAction>> GetAgentActionsAsync(string? agentName = null, int limit = 200)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var query = db.AgentActions.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(agentName)) query = query.Where(a => a.AgentName == agentName);
        return await query.OrderByDescending(a => a.Id).Take(limit).ToListAsync();
    }
}
