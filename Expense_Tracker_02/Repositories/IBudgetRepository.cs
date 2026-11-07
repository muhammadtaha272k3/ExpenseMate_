using Expense_Tracker_02.Models;

namespace Expense_Tracker_02.Repositories;

/// <summary>Data access for budgets. No business rules or calculations here.</summary>
public interface IBudgetRepository
{
    Task<List<Budget>> GetByMonthAsync(int month, int year);
    Task<Budget?> GetByIdAsync(int id);
    /// <summary>Finds the budget for a category/month/year (untracked), or null.</summary>
    Task<Budget?> FindAsync(string categoryName, int month, int year);
    Task AddAsync(Budget budget);
    Task UpdateAsync(Budget budget);
    Task DeleteAsync(Budget budget);
}
