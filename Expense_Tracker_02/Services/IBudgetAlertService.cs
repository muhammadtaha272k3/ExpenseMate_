namespace Expense_Tracker_02.Services;

public interface IBudgetAlertService
{
    /// <summary>Pure calculation: no database access. Applies the configured thresholds.</summary>
    BudgetStatus Evaluate(string categoryName, decimal limitAmount, decimal spent, int budgetId = 0);

    Task<List<BudgetStatus>> GetStatusForMonthAsync(int month, int year);

    Task<BudgetStatus?> CheckCategoryAsync(string categoryName, int month, int year);

    /// <summary>Alert text for over/near-limit statuses; null when no alert is needed.</summary>
    string? BuildAlertMessage(BudgetStatus? status);
}
