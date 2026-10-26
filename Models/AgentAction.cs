namespace FinanceAgent.Models
{
    public class AgentAction : IUserOwned
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string AgentName { get; set; } = string.Empty;   // "RuleBasedCategorizer", "AnomalyDetectionAgent", "User", etc.
        public string ActionType { get; set; } = string.Empty;  // "Categorized", "FlaggedAnomaly", "UpdatedBudget"
        public string Input { get; set; } = string.Empty;
        public string Output { get; set; } = string.Empty;
        public string Reasoning { get; set; } = string.Empty;
    }
}
