using Expense_Tracker_02.Models;
using Expense_Tracker_02.Services;
using Microsoft.AspNetCore.Mvc;

namespace Expense_Tracker_02.Controllers
{
    // Thin controller: validates basic input, calls the service, returns a result.
    // No DbContext, no SQL, no business rules.
    public class TransactionsController : Controller
    {
        private readonly ITransactionService _transactions;

        public TransactionsController(ITransactionService transactions)
        {
            _transactions = transactions;
        }

        // GET /Transactions?category=Food&type=Expense&from=...&to=...
        public async Task<IActionResult> Index(string? category, TransactionType? type, DateTime? from, DateTime? to)
        {
            ViewBag.Categories = Category.All;
            ViewBag.Category = category;
            ViewBag.Type = type;
            ViewBag.From = from?.ToString("yyyy-MM-dd");
            ViewBag.To = to?.ToString("yyyy-MM-dd");

            return View(await _transactions.GetFilteredAsync(category, type, from, to));
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

            var alert = await _transactions.AddAsync(transaction);
            if (alert != null) TempData["Alert"] = alert;

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var transaction = await _transactions.GetByIdAsync(id);
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

            await _transactions.UpdateAsync(transaction);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var transaction = await _transactions.GetByIdAsync(id);
            if (transaction == null) return NotFound();
            return View(transaction);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _transactions.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ExportCsv()
        {
            var bytes = await _transactions.ExportCsvAsync();
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
            var result = await _transactions.ImportCsvAsync(stream);

            TempData["Alert"] = $"Imported {result.Valid.Count} transaction(s)."
                + (result.Errors.Count > 0
                    ? $" {result.Errors.Count} row(s) skipped: {string.Join("; ", result.Errors.Take(5))}"
                    : "");

            return RedirectToAction(nameof(Index));
        }
    }
}
