using Expense_Tracker_02.Models;

namespace Expense_Tracker_02.Repositories;

/// <summary>Data access for transactions. No business rules or calculations here.</summary>
public interface ITransactionRepository
{
    Task<List<Transaction>> GetFilteredAsync(string? category, TransactionType? type, DateTime? from, DateTime? to);
    Task<List<Transaction>> GetAllAsync();
    Task<List<Transaction>> GetByMonthAsync(int month, int year);
    Task<List<Transaction>> GetExpensesForMonthAsync(int month, int year, string? category = null);
    Task<List<Transaction>> GetSinceAsync(DateTime from);
    Task<Transaction?> GetByIdAsync(int id);
    Task AddAsync(Transaction transaction);
    Task AddRangeAsync(IEnumerable<Transaction> transactions);
    Task UpdateAsync(Transaction transaction);
    Task DeleteAsync(Transaction transaction);
}
