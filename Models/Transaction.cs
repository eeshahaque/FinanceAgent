namespace FinanceAgent.Models
{
    public class Transaction : IUserOwned
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int AccountId { get; set; }
        public Account? Account { get; set; }

        public DateTime Date { get; set; }
        public string Merchant { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string RawDescription { get; set; } = string.Empty;

        // The uploaded file this row came from; null for rows imported before file history existed
        public int? ImportBatchId { get; set; }
        public ImportBatch? ImportBatch { get; set; }

        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        // Agent metadata — this is what makes it demoable/explainable
        public double? CategorizationConfidence { get; set; }
        public bool FlaggedAsAnomaly { get; set; }
        public string? AnomalyReason { get; set; }

        // Set when the user dismisses a flag so the anomaly agent doesn't re-raise it
        public bool AnomalyDismissed { get; set; }
    }
}
