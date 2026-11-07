using Expense_Tracker_02.Models;
using Expense_Tracker_02.Repositories;

namespace Expense_Tracker_02.Services;

public class BudgetService : IBudgetService
{
    private const string DuplicateMessage =
        "A budget for this category and month already exists — edit it instead.";

    private readonly IBudgetRepository _budgets;
    private readonly IBudgetAlertService _alerts;

    public BudgetService(IBudgetRepository budgets, IBudgetAlertService alerts)
    {
        _budgets = budgets;
        _alerts = alerts;
    }

    public Task<List<BudgetStatus>> GetStatusForMonthAsync(int month, int year) =>
        _alerts.GetStatusForMonthAsync(month, year);

    public Task<Budget?> GetByIdAsync(int id) => _budgets.GetByIdAsync(id);

    public async Task<ServiceResult> CreateAsync(Budget budget)
    {
        var existing = await _budgets.FindAsync(budget.CategoryName, budget.Month, budget.Year);
        if (existing != null) return ServiceResult.Fail(DuplicateMessage);

        await _budgets.AddAsync(budget);
        return ServiceResult.Ok();
    }

    // New rule: editing may not collide with a different budget (previously the unique index threw a 500).
    public async Task<ServiceResult> UpdateAsync(Budget budget)
    {
        var existing = await _budgets.FindAsync(budget.CategoryName, budget.Month, budget.Year);
        if (existing != null && existing.Id != budget.Id) return ServiceResult.Fail(DuplicateMessage);

        await _budgets.UpdateAsync(budget);
        return ServiceResult.Ok();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var budget = await _budgets.GetByIdAsync(id);
        if (budget == null) return false;

        await _budgets.DeleteAsync(budget);
        return true;
    }
}
