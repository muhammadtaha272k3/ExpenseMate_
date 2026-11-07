namespace Expense_Tracker_02.Services;

public class MonthlyReport
{
    public string[] CategoryLabels { get; set; } = Array.Empty<string>();
    public decimal[] CategoryTotals { get; set; } = Array.Empty<decimal>();
    public string[] TrendLabels { get; set; } = Array.Empty<string>();
    public decimal[] TrendIncome { get; set; } = Array.Empty<decimal>();
    public decimal[] TrendExpense { get; set; } = Array.Empty<decimal>();
}
