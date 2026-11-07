using Expense_Tracker_02.Data;
using Expense_Tracker_02.Models;
using Expense_Tracker_02.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker_02.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly BudgetAlertService _budgetAlerts;

        public HomeController(ApplicationDbContext db, BudgetAlertService budgetAlerts)
        {
            _db = db;
            _budgetAlerts = budgetAlerts;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            var monthTransactions = await _db.Transactions
                .Where(t => t.Date.Month == today.Month && t.Date.Year == today.Year)
                .ToListAsync();

            ViewBag.TotalIncome = monthTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            ViewBag.TotalExpense = monthTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
            ViewBag.RecentTransactions = monthTransactions.OrderByDescending(t => t.Date).Take(5).ToList();
            ViewBag.BudgetStatuses = await _budgetAlerts.GetStatusForMonthAsync(today.Month, today.Year);

            return View();
        }

        public IActionResult Error() => View();
    }
}
