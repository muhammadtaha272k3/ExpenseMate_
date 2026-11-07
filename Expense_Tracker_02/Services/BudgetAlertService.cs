using Expense_Tracker_02.Data;
using Expense_Tracker_02.Models;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker_02.Services
{
    public class BudgetStatus
    {
        public string CategoryName { get; set; } = "";
        public decimal LimitAmount { get; set; }
        public decimal Spent { get; set; }
        public decimal PercentUsed => LimitAmount == 0 ? 0 : Math.Round(Spent / LimitAmount * 100, 1);
        public bool IsNearLimit => PercentUsed is >= 80 and < 100;
        public bool IsOverLimit => PercentUsed >= 100;
    }

    public class BudgetAlertService
    {
        private readonly ApplicationDbContext _db;

        public BudgetAlertService(ApplicationDbContext db)
        {
            _db = db;
        }

        // Returns spend-vs-budget status for every budget defined in the given month/year.
        public async Task<List<BudgetStatus>> GetStatusForMonthAsync(int month, int year)
        {
            var budgets = await _db.Budgets
                .Where(b => b.Month == month && b.Year == year)
                .ToListAsync();

            var spendByCategory = await _db.Transactions
                .Where(t => t.Type == TransactionType.Expense && t.Date.Month == month && t.Date.Year == year)
                .GroupBy(t => t.CategoryName)
                .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) })
                .ToDictionaryAsync(x => x.Category, x => x.Total);

            return budgets.Select(b => new BudgetStatus
            {
                CategoryName = b.CategoryName,
                LimitAmount = b.LimitAmount,
                Spent = spendByCategory.TryGetValue(b.CategoryName, out var total) ? total : 0m
            }).ToList();
        }

        // Single-category check, used right after a new expense is saved so we can
        // flash an immediate alert instead of waiting for the dashboard to refresh.
        public async Task<BudgetStatus?> CheckCategoryAsync(string categoryName, int month, int year)
        {
            var budget = await _db.Budgets
                .FirstOrDefaultAsync(b => b.CategoryName == categoryName && b.Month == month && b.Year == year);
            if (budget == null) return null;

            var spent = await _db.Transactions
                .Where(t => t.Type == TransactionType.Expense
                    && t.CategoryName == categoryName
                    && t.Date.Month == month && t.Date.Year == year)
                .SumAsync(t => (decimal?)t.Amount) ?? 0m;

            return new BudgetStatus { CategoryName = categoryName, LimitAmount = budget.LimitAmount, Spent = spent };
        }
    }
}
