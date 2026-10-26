using FinanceAgent.Data;
using FinanceAgent.Models;
using FinanceAgent.Services;
using Microsoft.EntityFrameworkCore;

namespace FinanceAgent.Agents;

public record BudgetSuggestion(
    int CategoryId,
    string CategoryName,
    decimal CurrentBudget,
    decimal SuggestedBudget,
    decimal AverageSpend,
    int MonthsOverBudget,
    int MonthsAnalyzed,
    string Reason);

public record PaceAlert(string CategoryName, decimal Spent, decimal Budget, decimal ProjectedSpend, string Message);

// Looks at recent months of spending and proposes budgets that match reality.
// Suggestions are only applied when the user accepts them.
public class BudgetAdvisorAgent
{
    private const int LookbackMonths = 3;

    private readonly UserDbContextFactory _dbFactory;
    private readonly FinanceQueryService _queries;

    public BudgetAdvisorAgent(UserDbContextFactory dbFactory, FinanceQueryService queries)
    {
        _dbFactory = dbFactory;
        _queries = queries;
    }

    public async Task<List<BudgetSuggestion>> GetSuggestionsAsync()
    {
        var months = (await _queries.GetMonthsWithDataAsync()).Take(LookbackMonths).OrderBy(m => m).ToList();
        if (months.Count == 0) return new();

        var spend = await _queries.GetMonthlySpendByCategoryAsync(months);
        var categories = await _queries.GetCategoriesAsync();
        var suggestions = new List<BudgetSuggestion>();

        foreach (var category in categories.Where(c => !c.ExcludeFromBudget && c.Name != DbSeeder.Uncategorized))
        {
            var monthly = spend.GetValueOrDefault(category.Id) ?? new decimal[months.Count];
            var average = monthly.Average();
            var monthsOver = category.MonthlyBudget > 0 ? monthly.Count(m => m > category.MonthlyBudget) : 0;

            if (average == 0) continue;

            if (category.MonthlyBudget == 0)
            {
                suggestions.Add(Make(category, RoundUp(average * 1.1m), average, monthsOver, months.Count,
                    $"No budget set, but you spend about {average:C}/month here."));
            }
            else if (average > category.MonthlyBudget * 1.1m && monthsOver >= Math.Max(1, months.Count / 2))
            {
                suggestions.Add(Make(category, RoundUp(average), average, monthsOver, months.Count,
                    $"Over budget in {monthsOver} of the last {months.Count} months (avg {average:C} vs {category.MonthlyBudget:C}). " +
                    "Either raise the budget or cut back here."));
            }
            else if (average < category.MonthlyBudget * 0.6m && monthly.Max() < category.MonthlyBudget * 0.75m && months.Count >= 2)
            {
                // Size the cut to the busiest month, not the average, so a rising trend isn't squeezed
                var suggested = RoundUp(monthly.Max() * 1.1m);
                suggestions.Add(Make(category, suggested, average, monthsOver, months.Count,
                    $"You only use about {average / category.MonthlyBudget:P0} of this budget. " +
                    $"Lowering it frees {category.MonthlyBudget - suggested:C}/month for other goals."));
            }
        }

        return suggestions.OrderByDescending(s => Math.Abs(s.SuggestedBudget - s.CurrentBudget)).ToList();
    }

    // Categories on track to blow through their budget in the most recent month
    public async Task<List<PaceAlert>> GetPaceAlertsAsync()
    {
        var latest = (await _queries.GetMonthsWithDataAsync()).FirstOrDefault();
        if (latest == default) return new();

        var summary = await _queries.GetMonthSummaryAsync(latest.Year, latest.Month);
        var daysInMonth = DateTime.DaysInMonth(latest.Year, latest.Month);
        var today = DateTime.Today;
        var daysElapsed = latest.Year == today.Year && latest.Month == today.Month ? today.Day : daysInMonth;

        return summary.Categories
            .Where(c => c.Budget > 0 && !c.ExcludeFromBudget)
            .Select(c => new { c, Projected = c.Spent / daysElapsed * daysInMonth })
            .Where(x => x.c.OverBudget || x.Projected > x.c.Budget * 1.05m)
            .Select(x => new PaceAlert(x.c.Name, x.c.Spent, x.c.Budget, x.Projected,
                x.c.OverBudget
                    ? $"{x.c.Name} is over budget: {x.c.Spent:C} of {x.c.Budget:C} in {summary.Label}."
                    : $"{x.c.Name} is on pace for {x.Projected:C} against a {x.c.Budget:C} budget."))
            .OrderByDescending(a => a.Spent - a.Budget)
            .ToList();
    }

    public async Task<decimal> SetBudgetAsync(int categoryId, decimal newBudget, string reason, string actor)
    {
        if (newBudget < 0) throw new ArgumentOutOfRangeException(nameof(newBudget), "Budget can't be negative.");

        await using var db = await _dbFactory.CreateDbContextAsync();
        var category = await db.Categories.FindAsync(categoryId)
            ?? throw new InvalidOperationException($"Category {categoryId} not found.");

        var old = category.MonthlyBudget;
        category.MonthlyBudget = newBudget;
        db.AgentActions.Add(new AgentAction
        {
            AgentName = actor,
            ActionType = "UpdatedBudget",
            Input = $"{category.Name}: {old:C}",
            Output = $"{category.Name}: {newBudget:C}",
            Reasoning = reason
        });
        await db.SaveChangesAsync();
        return old;
    }

    private static BudgetSuggestion Make(Category c, decimal suggested, decimal avg, int over, int analyzed, string reason) =>
        new(c.Id, c.Name, c.MonthlyBudget, suggested, Math.Round(avg, 2), over, analyzed, reason);

    // Budgets read better as round numbers
    private static decimal RoundUp(decimal value) => Math.Ceiling(value / 10m) * 10m;
}
