namespace FinanceAgent.Agents;

public record PipelineResult(int RuleMatched, int AnomaliesFlagged);

// Runs the agents in order after an import: keyword rules first, then the anomaly scan
// over the categorized result.
public class AgentPipeline
{
    private readonly RuleBasedCategorizer _rules;
    private readonly AnomalyDetectionAgent _anomalies;

    public AgentPipeline(RuleBasedCategorizer rules, AnomalyDetectionAgent anomalies)
    {
        _rules = rules;
        _anomalies = anomalies;
    }

    public async Task<PipelineResult> RunAsync()
    {
        var matched = await _rules.CategorizeUncategorizedAsync();
        var flagged = await _anomalies.ScanAsync();
        return new PipelineResult(matched, flagged);
    }
}
