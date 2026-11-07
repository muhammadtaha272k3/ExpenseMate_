using Expense_Tracker_02.Services;
using Microsoft.AspNetCore.Mvc;

namespace Expense_Tracker_02.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IReportService _reports;

        public ReportsController(IReportService reports)
        {
            _reports = reports;
        }

        public async Task<IActionResult> Index(int? month, int? year)
        {
            var m = month ?? DateTime.Today.Month;
            var y = year ?? DateTime.Today.Year;
            ViewBag.Month = m;
            ViewBag.Year = y;

            var report = await _reports.GetMonthlyReportAsync(m, y);

            ViewBag.CategoryLabels = report.CategoryLabels;
            ViewBag.CategoryTotals = report.CategoryTotals;
            ViewBag.TrendLabels = report.TrendLabels;
            ViewBag.TrendIncome = report.TrendIncome;
            ViewBag.TrendExpense = report.TrendExpense;

            return View();
        }
    }
}
