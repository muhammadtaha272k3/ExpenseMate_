using Expense_Tracker_02.Models;

namespace Expense_Tracker_02.Services;

public class MonthSummary
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public List<Transaction> RecentTransactions { get; set; } = new();
}
