using Expense_Tracker_02.Data;
using Expense_Tracker_02.Models;
using Expense_Tracker_02.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker_02.Controllers
{
    public class BudgetsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly BudgetAlertService _budgetAlerts;

        public BudgetsController(ApplicationDbContext db, BudgetAlertService budgetAlerts)
        {
            _db = db;
            _budgetAlerts = budgetAlerts;
        }

        public async Task<IActionResult> Index(int? month, int? year)
        {
            var m = month ?? DateTime.Today.Month;
            var y = year ?? DateTime.Today.Year;

            ViewBag.Month = m;
            ViewBag.Year = y;

            return View(await _budgetAlerts.GetStatusForMonthAsync(m, y));
        }

        public IActionResult Create()
        {
            ViewBag.Categories = Category.All;
            return View(new Budget());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Budget budget)
        {
            bool exists = await _db.Budgets.AnyAsync(b =>
                b.CategoryName == budget.CategoryName && b.Month == budget.Month && b.Year == budget.Year);
            if (exists)
                ModelState.AddModelError("", "A budget for this category and month already exists — edit it instead.");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = Category.All;
                return View(budget);
            }

            _db.Add(budget);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { month = budget.Month, year = budget.Year });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var budget = await _db.Budgets.FindAsync(id);
            if (budget == null) return NotFound();
            ViewBag.Categories = Category.All;
            return View(budget);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Budget budget)
        {
            if (id != budget.Id) return NotFound();
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = Category.All;
                return View(budget);
            }

            _db.Update(budget);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { month = budget.Month, year = budget.Year });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var budget = await _db.Budgets.FindAsync(id);
            if (budget != null)
            {
                _db.Budgets.Remove(budget);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
