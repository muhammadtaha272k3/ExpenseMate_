using Expense_Tracker_02.Data;
using Expense_Tracker_02.Models;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker_02.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _db;

    public TransactionRepository(ApplicationDbContext db) => _db = db;

    public async Task<List<Transaction>> GetFilteredAsync(string? category, TransactionType? type, DateTime? from, DateTime? to)
    {
        var query = _db.Transactions.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(category)) query = query.Where(t => t.CategoryName == category);
        if (type.HasValue) query = query.Where(t => t.Type == type.Value);
        if (from.HasValue) query = query.Where(t => t.Date >= from.Value);
        if (to.HasValue) query = query.Where(t => t.Date <= to.Value);

        return await query.OrderByDescending(t => t.Date).ToListAsync();
    }

    public Task<List<Transaction>> GetAllAsync() =>
        _db.Transactions.AsNoTracking().OrderBy(t => t.Date).ToListAsync();

    public Task<List<Transaction>> GetByMonthAsync(int month, int year) =>
        _db.Transactions.AsNoTracking()
            .Where(t => t.Date.Month == month && t.Date.Year == year)
            .ToListAsync();

    public Task<List<Transaction>> GetExpensesForMonthAsync(int month, int year, string? category = null)
    {
        var query = _db.Transactions.AsNoTracking()
            .Where(t => t.Type == TransactionType.Expense && t.Date.Month == month && t.Date.Year == year);

        if (!string.IsNullOrEmpty(category)) query = query.Where(t => t.CategoryName == category);

        return query.ToListAsync();
    }

    public Task<List<Transaction>> GetSinceAsync(DateTime from) =>
        _db.Transactions.AsNoTracking().Where(t => t.Date >= from).ToListAsync();

    public Task<Transaction?> GetByIdAsync(int id) =>
        _db.Transactions.FirstOrDefaultAsync(t => t.Id == id);

    public async Task AddAsync(Transaction transaction)
    {
        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<Transaction> transactions)
    {
        _db.Transactions.AddRange(transactions);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Transaction transaction)
    {
        _db.Transactions.Update(transaction);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Transaction transaction)
    {
        _db.Transactions.Remove(transaction);
        await _db.SaveChangesAsync();
    }
}
