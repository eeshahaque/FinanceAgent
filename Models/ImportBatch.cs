namespace FinanceAgent.Models
{
    // One uploaded CSV file. Transactions point back to it so a whole file can be removed later.
    public class ImportBatch : IUserOwned
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int AccountId { get; set; }
        public Account? Account { get; set; }
        public string FileName { get; set; } = string.Empty;
        public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
        public int SkippedDuplicates { get; set; }
        public int SkippedInvalid { get; set; }
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
