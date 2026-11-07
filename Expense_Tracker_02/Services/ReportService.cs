using Expense_Tracker_02.Models;
using Expense_Tracker_02.Repositories;

namespace Expense_Tracker_02.Services;

public class ReportService : IReportService
{
    private readonly ITransactionRepository _transactions;

    public ReportService(ITransactionRepository transactions) => _transactions = transactions;

    public async Task<MonthlyReport> GetMonthlyReportAsync(int month, int year)
    {
        // Grouping/summing happens here (service), not in the repository.
        var expenses = await _transactions.GetExpensesForMonthAsync(month, year);
        var byCategory = expenses
            .GroupBy(t => t.CategoryName)
            .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) })
            .OrderByDescending(x => x.Total)
            .ToList();

        // Last 6 months trend (income vs expense), for the line chart.
        var start = new DateTime(year, month, 1).AddMonths(-5);
        var recent = await _transactions.GetSinceAsync(start);
        var months = Enumerable.Range(0, 6).Select(i => start.AddMonths(i)).ToList();

        decimal Total(DateTime m, TransactionType type) => recent
            .Where(t => t.Type == type && t.Date.Year == m.Year && t.Date.Month == m.Month)
            .Sum(t => t.Amount);

        return new MonthlyReport
        {
            CategoryLabels = byCategory.Select(x => x.Category).ToArray(),
            CategoryTotals = byCategory.Select(x => x.Total).ToArray(),
            TrendLabels = months.Select(m => m.ToString("MMM yyyy")).ToArray(),
            TrendIncome = months.Select(m => Total(m, TransactionType.Income)).ToArray(),
            TrendExpense = months.Select(m => Total(m, TransactionType.Expense)).ToArray()
        };
    }
}
