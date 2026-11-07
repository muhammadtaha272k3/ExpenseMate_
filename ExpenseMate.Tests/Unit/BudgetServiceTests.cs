using Expense_Tracker_02.Models;
using Expense_Tracker_02.Repositories;
using Expense_Tracker_02.Services;
using Moq;
using Xunit;

namespace ExpenseMate.Tests.Unit;

public class BudgetServiceTests
{
    private readonly Mock<IBudgetRepository> _repo = new();
    private readonly BudgetService _service;

    public BudgetServiceTests()
    {
        _service = new BudgetService(_repo.Object, Mock.Of<IBudgetAlertService>());
    }

    private static Budget B(int id = 0) =>
        new() { Id = id, CategoryName = "Food", Month = 11, Year = 2026, LimitAmount = 500m };

    [Fact]
    public async Task Create_NewBudget_Succeeds_AndSaves()
    {
        _repo.Setup(r => r.FindAsync("Food", 11, 2026)).ReturnsAsync((Budget?)null);

        var result = await _service.CreateAsync(B());

        Assert.True(result.Succeeded);
        _repo.Verify(r => r.AddAsync(It.IsAny<Budget>()), Times.Once);
    }

    [Fact]
    public async Task Create_Duplicate_IsRejected_AndNotSaved()
    {
        _repo.Setup(r => r.FindAsync("Food", 11, 2026)).ReturnsAsync(B(1));

        var result = await _service.CreateAsync(B());

        Assert.False(result.Succeeded);
        Assert.Contains("already exists", result.Error);
        _repo.Verify(r => r.AddAsync(It.IsAny<Budget>()), Times.Never);
    }

    [Fact]
    public async Task Update_SameRecord_IsAllowed()
    {
        _repo.Setup(r => r.FindAsync("Food", 11, 2026)).ReturnsAsync(B(5));

        var result = await _service.UpdateAsync(B(5));

        Assert.True(result.Succeeded);
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Budget>()), Times.Once);
    }

    [Fact]
    public async Task Update_CollidingWithOtherBudget_IsRejected()
    {
        _repo.Setup(r => r.FindAsync("Food", 11, 2026)).ReturnsAsync(B(2));

        var result = await _service.UpdateAsync(B(5));

        Assert.False(result.Succeeded);
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Budget>()), Times.Never);
    }

    [Fact]
    public async Task Delete_Missing_ReturnsFalse()
    {
        _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Budget?)null);

        Assert.False(await _service.DeleteAsync(99));
        _repo.Verify(r => r.DeleteAsync(It.IsAny<Budget>()), Times.Never);
    }
}
