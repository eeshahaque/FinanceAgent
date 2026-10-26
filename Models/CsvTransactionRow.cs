namespace FinanceAgent.Models;

// One normalized row from a bank CSV, whatever the bank's column layout was
public record CsvTransactionRow(DateTime Date, string Description, decimal Amount);
