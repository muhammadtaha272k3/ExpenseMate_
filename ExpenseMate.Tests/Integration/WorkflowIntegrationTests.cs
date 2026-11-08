using System.Text;
using Expense_Tracker_02.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ExpenseMate.Tests.Integration;

public class WorkflowIntegrationTests : IDisposable
{
    private readonly TestDb _env = new();
    private static readonly DateTime Nov = new(2026, 11, 3);

    public void Dispose() => _env.Dispose();

    private static Transaction Expense(string category, decimal amount, DateTime? date = null) =>
        new() { Type = TransactionType.Expense, Amount = amount, CategoryName = category, Date = date ?? Nov };

    private static Transaction Income(decimal amount, DateTime? date = null) =>
        new() { Type = TransactionType.Income, Amount = amount, CategoryName = "Salary", Date = date ?? Nov };

    // IT-01: Add expense that exceeds a budget -> saved + alert returned
    [Fact]
    public async Task AddExpense_OverBudget_PersistsAndReturnsAlert()
    {
        await _env.Budgets.CreateAsync(new Budget { CategoryName = "Food", Month = 11, Year = 2026, LimitAmount = 100m });

        var alert = await _env.Transactions.AddAsync(Expense("Food", 150m));

        Assert.NotNull(alert);
        Assert.Contains("Over budget on Food", alert);
        Assert.Equal(1, await _env.Db.Transactions.CountAsync());
    }

    // IT-02: Add income -> saved, no alert
    [Fact]
    public async Task AddIncome_PersistsWithoutAlert()
    {
        await _env.Budgets.CreateAsync(new Budget { CategoryName = "Salary", Month = 11, Year = 2026, LimitAmount = 1m });

        var alert = await _env.Transactions.AddAsync(Income(5000m));

        Assert.Null(alert);
        Assert.Equal(1, await _env.Db.Transactions.CountAsync());
    }

    // IT-03: Duplicate budget for same category/month is rejected
    [Fact]
    public async Task CreateBudget_Duplicate_IsRejected()
    {
        var first = await _env.Budgets.CreateAsync(new Budget { CategoryName = "Rent", Month = 11, Year = 2026, LimitAmount = 800m });
        var second = await _env.Budgets.CreateAsync(new Budget { CategoryName = "Rent", Month = 11, Year = 2026, LimitAmount = 900m });

        Assert.True(first.Succeeded);
        Assert.False(second.Succeeded);
        Assert.Equal(1, await _env.Db.Budgets.CountAsync());
    }

    // IT-04: Monthly budget summary reflects spending (Controller->Service->Repo->DB path used by Budgets/Index)
    [Fact]
    public async Task MonthlyBudgetSummary_ReflectsSpending()
    {
        await _env.Budgets.CreateAsync(new Budget { CategoryName = "Food", Month = 11, Year = 2026, LimitAmount = 200m });
        await _env.Transactions.AddAsync(Expense("Food", 60m));
        await _env.Transactions.AddAsync(Expense("Food", 40m));
        await _env.Transactions.AddAsync(Expense("Transport", 999m)); // different category, must not count

        var statuses = await _env.Budgets.GetStatusForMonthAsync(11, 2026);

        var food = Assert.Single(statuses);
        Assert.Equal(100m, food.Spent);
        Assert.Equal(50m, food.PercentUsed);
        Assert.False(food.IsNearLimit);
        Assert.False(food.IsOverLimit);
    }

    // IT-05: CSV import saves valid rows, skips bad ones
    [Fact]
    public async Task ImportCsv_SavesValidRows_SkipsBadRows()
    {
        var csv = "Type,Amount,Category,Date,Note\n" +
                  "Expense,20,Food,2026-11-01,ok\n" +
                  "Expense,-5,Food,2026-11-02,negative\n" +
                  "Income,300,Salary,2026-11-03,ok\n";

        var result = await _env.Transactions.ImportCsvAsync(new MemoryStream(Encoding.UTF8.GetBytes(csv)));

        Assert.Equal(2, result.Valid.Count);
        Assert.Single(result.Errors);
        Assert.Equal(2, await _env.Db.Transactions.CountAsync());
    }

    // IT-06: Delete removes the row and the dashboard summary updates
    [Fact]
    public async Task DeleteTransaction_RemovesRow_AndUpdatesSummary()
    {
        await _env.Transactions.AddAsync(Income(1000m));
        await _env.Transactions.AddAsync(Expense("Food", 100m));
        var expenseId = (await _env.Db.Transactions.FirstAsync(t => t.Type == TransactionType.Expense)).Id;

        var deleted = await _env.Transactions.DeleteAsync(expenseId);
        var summary = await _env.Transactions.GetMonthSummaryAsync(11, 2026);

        Assert.True(deleted);
        Assert.Equal(1000m, summary.TotalIncome);
        Assert.Equal(0m, summary.TotalExpense);
    }

    // IT-07: Dashboard totals and recent list
    [Fact]
    public async Task DashboardSummary_TotalsCorrect()
    {
        await _env.Transactions.AddAsync(Income(2000m));
        await _env.Transactions.AddAsync(Expense("Food", 300m));
        await _env.Transactions.AddAsync(Expense("Rent", 700m));
        await _env.Transactions.AddAsync(Expense("Food", 50m, new DateTime(2026, 10, 15))); // other month

        var summary = await _env.Transactions.GetMonthSummaryAsync(11, 2026);

        Assert.Equal(2000m, summary.TotalIncome);
        Assert.Equal(1000m, summary.TotalExpense);
        Assert.Equal(3, summary.RecentTransactions.Count);
    }

    // IT-08: Report groups by category, biggest first
    [Fact]
    public async Task Report_GroupsByCategory_SortedDescending()
    {
        await _env.Transactions.AddAsync(Expense("Food", 100m));
        await _env.Transactions.AddAsync(Expense("Food", 50m));
        await _env.Transactions.AddAsync(Expense("Rent", 800m));

        var report = await _env.Reports.GetMonthlyReportAsync(11, 2026);

        Assert.Equal(new[] { "Rent", "Food" }, report.CategoryLabels);
        Assert.Equal(new[] { 800m, 150m }, report.CategoryTotals);
        Assert.Equal(6, report.TrendLabels.Length);
    }

    // IT-09: Filter by category/type/date
    [Fact]
    public async Task Filter_ByCategoryAndType_ReturnsMatchesOnly()
    {
        await _env.Transactions.AddAsync(Expense("Food", 10m));
        await _env.Transactions.AddAsync(Expense("Transport", 20m));
        await _env.Transactions.AddAsync(Income(500m));

        var result = await _env.Transactions.GetFilteredAsync("Food", TransactionType.Expense, null, null);

        var only = Assert.Single(result);
        Assert.Equal("Food", only.CategoryName);
    }

    // IT-10: Budget summary exposes the budget id, so the page can link to Edit/Delete
    [Fact]
    public async Task MonthlyBudgetSummary_CarriesBudgetId()
    {
        await _env.Budgets.CreateAsync(new Budget { CategoryName = "Food", Month = 11, Year = 2026, LimitAmount = 200m });
        var saved = await _env.Db.Budgets.FirstAsync();

        var statuses = await _env.Budgets.GetStatusForMonthAsync(11, 2026);

        Assert.Equal(saved.Id, Assert.Single(statuses).Id);
    }

    // IT-11: Editing a budget's limit keeps one row and updates the amount
    [Fact]
    public async Task EditBudget_UpdatesLimit_WithoutDuplicate()
    {
        await _env.Budgets.CreateAsync(new Budget { CategoryName = "Food", Month = 11, Year = 2026, LimitAmount = 200m });
        var id = (await _env.Db.Budgets.AsNoTracking().FirstAsync()).Id;
        _env.Db.ChangeTracker.Clear(); // a real edit arrives in a new request, i.e. a new DbContext

        var result = await _env.Budgets.UpdateAsync(
            new Budget { Id = id, CategoryName = "Food", Month = 11, Year = 2026, LimitAmount = 350m });

        Assert.True(result.Succeeded);
        Assert.Equal(1, await _env.Db.Budgets.CountAsync());
        Assert.Equal(350m, (await _env.Db.Budgets.AsNoTracking().FirstAsync()).LimitAmount);
    }
}
