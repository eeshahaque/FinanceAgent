using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceAgent.Models
{
    public enum CategoryKind
    {
        Expense = 0,
        Income = 1,
        Transfer = 2
    }

    public class Category : IUserOwned
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string Name { get; set; } = string.Empty;   // e.g. "Dining", "Groceries"
        public decimal MonthlyBudget { get; set; }
        public CategoryKind Kind { get; set; } = CategoryKind.Expense;

        // Income and transfers aren't "spending", so they're left out of budget math
        [NotMapped]
        public bool ExcludeFromBudget => Kind != CategoryKind.Expense;

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
