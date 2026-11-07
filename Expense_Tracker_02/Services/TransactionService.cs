using Expense_Tracker_02.Models;
using Expense_Tracker_02.Repositories;

namespace Expense_Tracker_02.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactions;
    private readonly IBudgetAlertService _alerts;
    private readonly ICsvExportService _csvExport;
    private readonly ICsvImportService _csvImport;

    public TransactionService(
        ITransactionRepository transactions,
        IBudgetAlertService alerts,
        ICsvExportService csvExport,
        ICsvImportService csvImport)
    {
        _transactions = transactions;
        _alerts = alerts;
        _csvExport = csvExport;
        _csvImport = csvImport;
    }

    public Task<List<Transaction>> GetFilteredAsync(string? category, TransactionType? type, DateTime? from, DateTime? to) =>
        _transactions.GetFilteredAsync(category, type, from, to);

    public Task<Transaction?> GetByIdAsync(int id) => _transactions.GetByIdAsync(id);

    public async Task<string?> AddAsync(Transaction transaction)
    {
        await _transactions.AddAsync(transaction);

        // Immediate budget-alert feedback for expenses (Assignment 01 "budget alerts" requirement).
        if (transaction.Type != TransactionType.Expense) return null;

        var status = await _alerts.CheckCategoryAsync(
            transaction.CategoryName, transaction.Date.Month, transaction.Date.Year);
        return _alerts.BuildAlertMessage(status);
    }

    public Task UpdateAsync(Transaction transaction) => _transactions.UpdateAsync(transaction);

    public async Task<bool> DeleteAsync(int id)
    {
        var transaction = await _transactions.GetByIdAsync(id);
        if (transaction == null) return false;

        await _transactions.DeleteAsync(transaction);
        return true;
    }

    public async Task<MonthSummary> GetMonthSummaryAsync(int month, int year)
    {
        var monthTransactions = await _transactions.GetByMonthAsync(month, year);

        return new MonthSummary
        {
            TotalIncome = monthTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
            TotalExpense = monthTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount),
            RecentTransactions = monthTransactions.OrderByDescending(t => t.Date).Take(5).ToList()
        };
    }

    public async Task<byte[]> ExportCsvAsync()
    {
        var all = await _transactions.GetAllAsync();
        return _csvExport.Export(all);
    }

    public async Task<CsvImportResult> ImportCsvAsync(Stream fileStream)
    {
        var result = _csvImport.Import(fileStream);
        if (result.Valid.Count > 0)
            await _transactions.AddRangeAsync(result.Valid);
        return result;
    }
}
