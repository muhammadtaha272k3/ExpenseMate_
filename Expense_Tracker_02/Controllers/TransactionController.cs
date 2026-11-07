using Expense_Tracker_02.Data;
using Expense_Tracker_02.Models;
using Expense_Tracker_02.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker_02.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly CsvService _csv;
        private readonly BudgetAlertService _budgetAlerts;

        public TransactionsController(ApplicationDbContext db, CsvService csv, BudgetAlertService budgetAlerts)
        {
            _db = db;
            _csv = csv;
            _budgetAlerts = budgetAlerts;
        }

        // GET /Transactions?category=Food&type=Expense&from=...&to=...
        public async Task<IActionResult> Index(string? category, TransactionType? type, DateTime? from, DateTime? to)
        {
            var query = _db.Transactions.AsQueryable();

            if (!string.IsNullOrEmpty(category)) query = query.Where(t => t.CategoryName == category);
            if (type.HasValue) query = query.Where(t => t.Type == type.Value);
            if (from.HasValue) query = query.Where(t => t.Date >= from.Value);
            if (to.HasValue) query = query.Where(t => t.Date <= to.Value);

            ViewBag.Categories = Category.All;
            ViewBag.Category = category;
            ViewBag.Type = type;
            ViewBag.From = from?.ToString("yyyy-MM-dd");
            ViewBag.To = to?.ToString("yyyy-MM-dd");

            return View(await query.OrderByDescending(t => t.Date).ToListAsync());
        }

        public IActionResult Create()
        {
            ViewBag.Categories = Category.All;
            return View(new Transaction());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Transaction transaction)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = Category.All;
                return View(transaction);
            }

            _db.Add(transaction);
            await _db.SaveChangesAsync();

            // Immediate budget-alert feedback for expenses, per the assignment's
            // "budget alerts" requirement.
            if (transaction.Type == TransactionType.Expense)
            {
                var status = await _budgetAlerts.CheckCategoryAsync(transaction.CategoryName, transaction.Date.Month, transaction.Date.Year);
                if (status != null && status.IsOverLimit)
                    TempData["Alert"] = $"Over budget on {status.CategoryName}: {status.Spent:C} of {status.LimitAmount:C} spent.";
                else if (status != null && status.IsNearLimit)
                    TempData["Alert"] = $"Nearing budget on {status.CategoryName}: {status.PercentUsed}% used.";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var transaction = await _db.Transactions.FindAsync(id);
            if (transaction == null) return NotFound();
            ViewBag.Categories = Category.All;
            return View(transaction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Transaction transaction)
        {
            if (id != transaction.Id) return NotFound();
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = Category.All;
                return View(transaction);
            }

            _db.Update(transaction);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var transaction = await _db.Transactions.FindAsync(id);
            if (transaction == null) return NotFound();
            return View(transaction);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _db.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _db.Transactions.Remove(transaction);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ExportCsv()
        {
            var transactions = await _db.Transactions.OrderBy(t => t.Date).ToListAsync();
            var bytes = _csv.Export(transactions);
            return File(bytes, "text/csv", $"expensemate_export_{DateTime.Today:yyyyMMdd}.csv");
        }

        public IActionResult ImportCsv() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please choose a CSV file.");
                return View();
            }

            using var stream = file.OpenReadStream();
            var (valid, errors) = _csv.Import(stream);

            if (valid.Count > 0)
            {
                _db.Transactions.AddRange(valid);
                await _db.SaveChangesAsync();
            }

            TempData["Alert"] = $"Imported {valid.Count} transaction(s)."
                + (errors.Count > 0 ? $" {errors.Count} row(s) skipped: {string.Join("; ", errors.Take(5))}" : "");

            return RedirectToAction(nameof(Index));
        }
    }
}
