using FinanceAgent.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceAgent.Data;

public static class DbSeeder
{
    public const string Uncategorized = "Uncategorized";

    private static readonly (string Name, decimal Budget, CategoryKind Kind)[] DefaultCategories =
    {
        ("Dining", 300, CategoryKind.Expense),
        ("Groceries", 400, CategoryKind.Expense),
        ("Gas", 150, CategoryKind.Expense),
        ("Transportation", 100, CategoryKind.Expense),
        ("Subscriptions", 50, CategoryKind.Expense),
        ("Shopping", 200, CategoryKind.Expense),
        ("Health & Fitness", 75, CategoryKind.Expense),
        ("Utilities", 200, CategoryKind.Expense),
        ("Entertainment", 100, CategoryKind.Expense),
        ("Income", 0, CategoryKind.Income),
        ("Transfers", 0, CategoryKind.Transfer),
        (Uncategorized, 0, CategoryKind.Expense),
    };

    public static async Task MigrateAsync(FinanceContext db) => await db.Database.MigrateAsync();

    // Gives a newly registered user a checking account and the default categories.
    // The first user also adopts any data created before accounts existed.
    public static async Task SeedNewUserAsync(FinanceContext db, string userId, bool adoptUnownedData)
    {
        if (adoptUnownedData)
        {
            await db.Accounts.IgnoreQueryFilters().Where(e => e.UserId == null).ExecuteUpdateAsync(s => s.SetProperty(e => e.UserId, userId));
            await db.Categories.IgnoreQueryFilters().Where(e => e.UserId == null).ExecuteUpdateAsync(s => s.SetProperty(e => e.UserId, userId));
            await db.Transactions.IgnoreQueryFilters().Where(e => e.UserId == null).ExecuteUpdateAsync(s => s.SetProperty(e => e.UserId, userId));
            await db.CategoryRules.IgnoreQueryFilters().Where(e => e.UserId == null).ExecuteUpdateAsync(s => s.SetProperty(e => e.UserId, userId));
            await db.AgentActions.IgnoreQueryFilters().Where(e => e.UserId == null).ExecuteUpdateAsync(s => s.SetProperty(e => e.UserId, userId));
        }

        db.CurrentUserId = userId;

        if (!await db.Accounts.AnyAsync())
        {
            db.Accounts.Add(new Account { Name = "Checking", Institution = "My Bank", Balance = 0 });
        }

        // Adopted data keeps its own categories; only missing defaults are added
        var existing = await db.Categories.Select(c => c.Name).ToListAsync();
        foreach (var (name, budget, kind) in DefaultCategories.Where(d => !existing.Contains(d.Name)))
        {
            db.Categories.Add(new Category { Name = name, MonthlyBudget = budget, Kind = kind });
        }

        await db.SaveChangesAsync();
    }
}
