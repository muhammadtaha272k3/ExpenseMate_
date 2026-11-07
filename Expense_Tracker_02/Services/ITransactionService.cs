using Expense_Tracker_02.Models;

namespace Expense_Tracker_02.Services;

public interface ITransactionService
{
    Task<List<Transaction>> GetFilteredAsync(string? category, TransactionType? type, DateTime? from, DateTime? to);
    Task<Transaction?> GetByIdAsync(int id);

    /// <summary>Saves the transaction. Returns a budget-alert message for expenses that hit a limit, otherwise null.</summary>
    Task<string?> AddAsync(Transaction transaction);

    Task UpdateAsync(Transaction transaction);
    Task<bool> DeleteAsync(int id);

    Task<MonthSummary> GetMonthSummaryAsync(int month, int year);

    Task<byte[]> ExportCsvAsync();
    Task<CsvImportResult> ImportCsvAsync(Stream fileStream);
}
