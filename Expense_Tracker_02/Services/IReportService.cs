namespace Expense_Tracker_02.Services;

public interface IReportService
{
    Task<MonthlyReport> GetMonthlyReportAsync(int month, int year);
}
