namespace Expense_Tracker_02.Services;

// Same public shape the views already use. Flags are now set by BudgetAlertService
// from configured thresholds instead of being hard-coded here.
public class BudgetStatus
{
    public int Id { get; set; }   // budget id, so the Budgets page can link to Edit/Delete
    public string CategoryName { get; set; } = "";
    public decimal LimitAmount { get; set; }
    public decimal Spent { get; set; }
    public decimal PercentUsed => LimitAmount == 0 ? 0 : Math.Round(Spent / LimitAmount * 100, 1);
    public bool IsNearLimit { get; set; }
    public bool IsOverLimit { get; set; }
}
