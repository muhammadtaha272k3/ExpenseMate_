using Expense_Tracker_02.Repositories;
using Microsoft.Extensions.Options;

namespace Expense_Tracker_02.Services;

/// <summary>
/// REWRITTEN. Old version queried ApplicationDbContext directly, hard-coded the 80/100 thresholds
/// inside BudgetStatus, duplicated the "spent" query in two methods, and left alert text to the controller.
/// </summary>
public class BudgetAlertService : IBudgetAlertService
{
    private readonly IBudgetRepository _budgets;
    private readonly ITransactionRepository _transactions;
    private readonly BudgetAlertOptions _options;

    public BudgetAlertService(
        IBudgetRepository budgets,
        ITransactionRepository transactions,
        IOptions<BudgetAlertOptions> options)
    {
        _budgets = budgets;
        _transactions = transactions;
        _options = options.Value;
    }

    public BudgetStatus Evaluate(string categoryName, decimal limitAmount, decimal spent)
    {
        var status = new BudgetStatus
        {
            CategoryName = categoryName,
            LimitAmount = limitAmount,
            Spent = spent
        };

        var percent = status.PercentUsed;
        status.IsOverLimit = percent >= _options.OverLimitPercent;
        status.IsNearLimit = !status.IsOverLimit && percent >= _options.NearLimitPercent;
        return status;
    }

    public async Task<List<BudgetStatus>> GetStatusForMonthAsync(int month, int year)
    {
        var budgets = await _budgets.GetByMonthAsync(month, year);
        var expenses = await _transactions.GetExpensesForMonthAsync(month, year);

        var spendByCategory = expenses
            .GroupBy(t => t.CategoryName)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

        return budgets
            .Select(b => Evaluate(b.CategoryName, b.LimitAmount,
                spendByCategory.TryGetValue(b.CategoryName, out var total) ? total : 0m))
            .ToList();
    }

    public async Task<BudgetStatus?> CheckCategoryAsync(string categoryName, int month, int year)
    {
        var budget = await _budgets.FindAsync(categoryName, month, year);
        if (budget == null) return null;

        var expenses = await _transactions.GetExpensesForMonthAsync(month, year, categoryName);
        return Evaluate(categoryName, budget.LimitAmount, expenses.Sum(t => t.Amount));
    }

    public string? BuildAlertMessage(BudgetStatus? status)
    {
        if (status == null) return null;

        if (status.IsOverLimit)
            return $"Over budget on {status.CategoryName}: {CurrencyHelper.Format(status.Spent)} of {CurrencyHelper.Format(status.LimitAmount)} spent.";

        if (status.IsNearLimit)
            return $"Nearing budget on {status.CategoryName}: {status.PercentUsed}% used.";

        return null;
    }
}
