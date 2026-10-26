using FinanceAgent.Data;
using FinanceAgent.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceAgent.Services;

public record CategoryUsage(Category Category, int TransactionCount, int RuleCount)
{
    public bool IsSystem => Category.Name == DbSeeder.Uncategorized;
}

// Add, rename, and delete the signed-in user's categories
public class CategoryService
{
    public const int MaxNameLength = 100;

    private readonly UserDbContextFactory _dbFactory;

    public CategoryService(UserDbContextFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<CategoryUsage>> GetCategoriesWithUsageAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var txnCounts = await db.Transactions.Where(t => t.CategoryId != null)
            .GroupBy(t => t.CategoryId!.Value).Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);
        var ruleCounts = await db.CategoryRules
            .GroupBy(r => r.CategoryId).Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

        var categories = await db.Categories.AsNoTracking().OrderBy(c => c.Kind).ThenBy(c => c.Name).ToListAsync();
        return categories
            .Select(c => new CategoryUsage(c, txnCounts.GetValueOrDefault(c.Id), ruleCounts.GetValueOrDefault(c.Id)))
            .ToList();
    }

    public async Task<Category> AddAsync(string name, CategoryKind kind, decimal monthlyBudget)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        name = await ValidateNameAsync(db, name, excludeId: null);

        var category = new Category
        {
            Name = name,
            Kind = kind,
            MonthlyBudget = kind == CategoryKind.Expense ? Math.Max(0, monthlyBudget) : 0
        };
        db.Categories.Add(category);
        Log(db, "AddedCategory", name, $"{kind}, budget {category.MonthlyBudget:C}");
        await db.SaveChangesAsync();
        return category;
    }

    public async Task UpdateAsync(int id, string name, CategoryKind kind, decimal monthlyBudget)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new InvalidOperationException("That category no longer exists.");

        if (category.Name == DbSeeder.Uncategorized && (name.Trim() != DbSeeder.Uncategorized || kind != CategoryKind.Expense))
            throw new InvalidOperationException("Uncategorized is used for the review queue and can't be renamed or changed.");

        var oldName = category.Name;
        category.Name = await ValidateNameAsync(db, name, excludeId: id);
        category.Kind = kind;
        category.MonthlyBudget = kind == CategoryKind.Expense ? Math.Max(0, monthlyBudget) : 0;

        Log(db, "UpdatedCategory", oldName, $"{category.Name}, {kind}, budget {category.MonthlyBudget:C}");
        await db.SaveChangesAsync();
    }

    // Deletes a category after moving its transactions (and learned merchant rules) to another category
    public async Task<int> DeleteAsync(int id, int moveToCategoryId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new InvalidOperationException("That category no longer exists.");
        if (category.Name == DbSeeder.Uncategorized)
            throw new InvalidOperationException("Uncategorized is used for the review queue and can't be deleted.");
        if (moveToCategoryId == id)
            throw new InvalidOperationException("Pick a different category to move the transactions to.");

        var target = await db.Categories.FirstOrDefaultAsync(c => c.Id == moveToCategoryId)
            ?? throw new InvalidOperationException("The category to move transactions to doesn't exist.");

        await using var tx = await db.Database.BeginTransactionAsync();

        var moved = await db.Transactions.Where(t => t.CategoryId == id)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.CategoryId, target.Id));

        // A merchant rule pointing at Uncategorized would be pointless, so those are dropped instead
        if (target.Name == DbSeeder.Uncategorized)
            await db.CategoryRules.Where(r => r.CategoryId == id).ExecuteDeleteAsync();
        else
            await db.CategoryRules.Where(r => r.CategoryId == id)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.CategoryId, target.Id));

        db.Categories.Remove(category);
        Log(db, "DeletedCategory", category.Name, $"{moved} transactions moved to {target.Name}");
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return moved;
    }

    private static async Task<string> ValidateNameAsync(FinanceContext db, string name, int? excludeId)
    {
        name = name?.Trim() ?? string.Empty;
        if (name.Length == 0) throw new InvalidOperationException("Category name can't be empty.");
        if (name.Length > MaxNameLength) throw new InvalidOperationException($"Category name must be {MaxNameLength} characters or fewer.");

        // SQL Server's default collation is case-insensitive, so "dining" matches "Dining" here
        if (await db.Categories.AnyAsync(c => c.Name == name && c.Id != excludeId))
            throw new InvalidOperationException($"You already have a category named \"{name}\".");
        return name;
    }

    private static void Log(FinanceContext db, string action, string input, string output) =>
        db.AgentActions.Add(new AgentAction
        {
            AgentName = "User",
            ActionType = action,
            Input = input,
            Output = output,
            Reasoning = "Changed on the Categories page"
        });
}
