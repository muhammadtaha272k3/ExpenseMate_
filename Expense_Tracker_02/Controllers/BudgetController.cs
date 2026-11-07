using Expense_Tracker_02.Models;
using Expense_Tracker_02.Services;
using Microsoft.AspNetCore.Mvc;

namespace Expense_Tracker_02.Controllers
{
    public class BudgetsController : Controller
    {
        private readonly IBudgetService _budgets;

        public BudgetsController(IBudgetService budgets)
        {
            _budgets = budgets;
        }

        public async Task<IActionResult> Index(int? month, int? year)
        {
            var m = month ?? DateTime.Today.Month;
            var y = year ?? DateTime.Today.Year;

            ViewBag.Month = m;
            ViewBag.Year = y;

            return View(await _budgets.GetStatusForMonthAsync(m, y));
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
            if (ModelState.IsValid)
            {
                var result = await _budgets.CreateAsync(budget);
                if (result.Succeeded)
                    return RedirectToAction(nameof(Index), new { month = budget.Month, year = budget.Year });

                ModelState.AddModelError("", result.Error!);
            }

            ViewBag.Categories = Category.All;
            return View(budget);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var budget = await _budgets.GetByIdAsync(id);
            if (budget == null) return NotFound();
            ViewBag.Categories = Category.All;
            return View(budget);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Budget budget)
        {
            if (id != budget.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var result = await _budgets.UpdateAsync(budget);
                if (result.Succeeded)
                    return RedirectToAction(nameof(Index), new { month = budget.Month, year = budget.Year });

                ModelState.AddModelError("", result.Error!);
            }

            ViewBag.Categories = Category.All;
            return View(budget);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _budgets.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
