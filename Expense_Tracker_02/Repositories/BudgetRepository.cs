using Expense_Tracker_02.Data;
using Expense_Tracker_02.Models;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker_02.Repositories;

public class BudgetRepository : IBudgetRepository
{
    private readonly ApplicationDbContext _db;

    public BudgetRepository(ApplicationDbContext db) => _db = db;

    public Task<List<Budget>> GetByMonthAsync(int month, int year) =>
        _db.Budgets.AsNoTracking().Where(b => b.Month == month && b.Year == year).ToListAsync();

    public Task<Budget?> GetByIdAsync(int id) =>
        _db.Budgets.FirstOrDefaultAsync(b => b.Id == id);

    // AsNoTracking so a later Update() of a detached instance with the same Id does not clash.
    public Task<Budget?> FindAsync(string categoryName, int month, int year) =>
        _db.Budgets.AsNoTracking().FirstOrDefaultAsync(b =>
            b.CategoryName == categoryName && b.Month == month && b.Year == year);

    public async Task AddAsync(Budget budget)
    {
        _db.Budgets.Add(budget);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Budget budget)
    {
        _db.Budgets.Update(budget);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Budget budget)
    {
        _db.Budgets.Remove(budget);
        await _db.SaveChangesAsync();
    }
}
