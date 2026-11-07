namespace Expense_Tracker_02.Services;

/// <summary>Bound from appsettings.json section "BudgetAlerts".</summary>
public class BudgetAlertOptions
{
    public decimal NearLimitPercent { get; set; } = 80;
    public decimal OverLimitPercent { get; set; } = 100;
}
