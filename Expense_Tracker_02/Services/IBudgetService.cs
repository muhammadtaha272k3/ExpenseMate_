using Expense_Tracker_02.Models;

namespace Expense_Tracker_02.Services;

public interface IBudgetService
{
    Task<List<BudgetStatus>> GetStatusForMonthAsync(int month, int year);
    Task<Budget?> GetByIdAsync(int id);
    Task<ServiceResult> CreateAsync(Budget budget);
    Task<ServiceResult> UpdateAsync(Budget budget);
    Task<bool> DeleteAsync(int id);
}
