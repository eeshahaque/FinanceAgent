namespace FinanceAgent.Models
{
    // Keyword rule learned from a user correction. Checked before the built-in rules.
    public class CategoryRule : IUserOwned
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string Keyword { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
