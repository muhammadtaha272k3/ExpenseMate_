using Expense_Tracker_02.Services;
using Microsoft.AspNetCore.Mvc;

namespace Expense_Tracker_02.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITransactionService _transactions;
        private readonly IBudgetService _budgets;

        public HomeController(ITransactionService transactions, IBudgetService budgets)
        {
            _transactions = transactions;
            _budgets = budgets;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            var summary = await _transactions.GetMonthSummaryAsync(today.Month, today.Year);

            // Same ViewBag keys/types as before so the dashboard view is unchanged.
            ViewBag.TotalIncome = summary.TotalIncome;
            ViewBag.TotalExpense = summary.TotalExpense;
            ViewBag.RecentTransactions = summary.RecentTransactions;
            ViewBag.BudgetStatuses = await _budgets.GetStatusForMonthAsync(today.Month, today.Year);

            return View();
        }

        public IActionResult Error() => View();
    }
}
