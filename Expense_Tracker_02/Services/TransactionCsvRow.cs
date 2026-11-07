namespace Expense_Tracker_02.Services;

public class TransactionCsvRow
{
    public string Type { get; set; } = "";
    public decimal Amount { get; set; }
    public string Category { get; set; } = "";
    public string Date { get; set; } = "";
    public string? Note { get; set; }
}
