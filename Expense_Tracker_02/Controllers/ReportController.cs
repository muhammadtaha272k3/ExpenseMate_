using Expense_Tracker_02.Data;
using Expense_Tracker_02.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker_02.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ReportsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(int? month, int? year)
        {
            var m = month ?? DateTime.Today.Month;
            var y = year ?? DateTime.Today.Year;
            ViewBag.Month = m;
            ViewBag.Year = y;

            var expensesByCategory = await _db.Transactions
                .Where(t => t.Type == TransactionType.Expense && t.Date.Month == m && t.Date.Year == y)
                .GroupBy(t => t.CategoryName)
                .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            // Last 6 months trend (income vs expense), for the line chart.
            var sixMonthsAgo = new DateTime(y, m, 1).AddMonths(-5);
            var monthlyTrend = await _db.Transactions
                .Where(t => t.Date >= sixMonthsAgo)
                .GroupBy(t => new { t.Date.Year, t.Date.Month, t.Type })
                .Select(g => new { g.Key.Year, g.Key.Month, g.Key.Type, Total = g.Sum(t => t.Amount) })
                .ToListAsync();

            ViewBag.CategoryLabels = expensesByCategory.Select(x => x.Category).ToArray();
            ViewBag.CategoryTotals = expensesByCategory.Select(x => x.Total).ToArray();

            var months = Enumerable.Range(0, 6)
                .Select(i => sixMonthsAgo.AddMonths(i))
                .ToList();
            ViewBag.TrendLabels = months.Select(dt => dt.ToString("MMM yyyy")).ToArray();
            ViewBag.TrendIncome = months
                .Select(dt => monthlyTrend.Where(x => x.Year == dt.Year && x.Month == dt.Month && x.Type == TransactionType.Income).Sum(x => x.Total))
                .ToArray();
            ViewBag.TrendExpense = months
                .Select(dt => monthlyTrend.Where(x => x.Year == dt.Year && x.Month == dt.Month && x.Type == TransactionType.Expense).Sum(x => x.Total))
                .ToArray();

            return View();
        }
    }
}
