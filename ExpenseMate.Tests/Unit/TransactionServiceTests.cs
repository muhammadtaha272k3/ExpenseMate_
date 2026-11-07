using Expense_Tracker_02.Models;
using Expense_Tracker_02.Repositories;
using Expense_Tracker_02.Services;
using Moq;
using Xunit;

namespace ExpenseMate.Tests.Unit;

public class TransactionServiceTests
{
    private readonly Mock<ITransactionRepository> _repo = new();
    private readonly Mock<IBudgetAlertService> _alerts = new();
    private readonly TransactionService _service;

    public TransactionServiceTests()
    {
        _service = new TransactionService(
            _repo.Object, _alerts.Object, new CsvExportService(), new CsvImportService(new CsvRowValidator()));
    }

    private static Transaction Tx(TransactionType type) => new()
    {
        Type = type, Amount = 50m, CategoryName = "Food", Date = new DateTime(2026, 11, 3)
    };

    [Fact]
    public async Task AddAsync_Expense_ChecksBudget_AndReturnsAlert()
    {
        var status = new BudgetStatus { CategoryName = "Food", IsOverLimit = true };
        _alerts.Setup(a => a.CheckCategoryAsync("Food", 11, 2026)).ReturnsAsync(status);
        _alerts.Setup(a => a.BuildAlertMessage(status)).Returns("Over budget on Food");

        var alert = await _service.AddAsync(Tx(TransactionType.Expense));

        Assert.Equal("Over budget on Food", alert);
        _repo.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_Income_SkipsBudgetCheck()
    {
        var alert = await _service.AddAsync(Tx(TransactionType.Income));

        Assert.Null(alert);
        _alerts.Verify(a => a.CheckCategoryAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _repo.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Once);
    }

    [Fact]
    public async Task GetMonthSummary_SumsIncomeAndExpense_AndTakesFiveRecent()
    {
        var items = new List<Transaction>();
        for (int i = 1; i <= 7; i++)
            items.Add(new Transaction { Type = TransactionType.Expense, Amount = 10, CategoryName = "Food", Date = new DateTime(2026, 11, i) });
        items.Add(new Transaction { Type = TransactionType.Income, Amount = 1000, CategoryName = "Salary", Date = new DateTime(2026, 11, 1) });
        _repo.Setup(r => r.GetByMonthAsync(11, 2026)).ReturnsAsync(items);

        var summary = await _service.GetMonthSummaryAsync(11, 2026);

        Assert.Equal(1000m, summary.TotalIncome);
        Assert.Equal(70m, summary.TotalExpense);
        Assert.Equal(5, summary.RecentTransactions.Count);
        Assert.Equal(new DateTime(2026, 11, 7), summary.RecentTransactions[0].Date);
    }

    [Fact]
    public async Task DeleteAsync_Missing_ReturnsFalse()
    {
        _repo.Setup(r => r.GetByIdAsync(42)).ReturnsAsync((Transaction?)null);

        Assert.False(await _service.DeleteAsync(42));
        _repo.Verify(r => r.DeleteAsync(It.IsAny<Transaction>()), Times.Never);
    }
}
