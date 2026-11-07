using Expense_Tracker_02.Repositories;
using Expense_Tracker_02.Services;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ExpenseMate.Tests.Unit;

public class BudgetAlertServiceTests
{
    private static BudgetAlertService Create(BudgetAlertOptions? options = null) =>
        new(Mock.Of<IBudgetRepository>(),
            Mock.Of<ITransactionRepository>(),
            Options.Create(options ?? new BudgetAlertOptions()));

    [Theory]
    [InlineData(1000, 790, false, false)]   // 79%  -> fine
    [InlineData(1000, 800, true, false)]    // 80%  -> near
    [InlineData(1000, 999, true, false)]    // 99.9% -> near
    [InlineData(1000, 1000, false, true)]   // 100% -> over
    [InlineData(1000, 1500, false, true)]   // 150% -> over
    [InlineData(0, 50, false, false)]       // zero limit -> no division error
    public void Evaluate_AppliesDefaultThresholds(int limit, int spent, bool near, bool over)
    {
        var status = Create().Evaluate("Food", limit, spent);

        Assert.Equal(near, status.IsNearLimit);
        Assert.Equal(over, status.IsOverLimit);
    }

    [Fact]
    public void Evaluate_UsesConfiguredThresholds()
    {
        var service = Create(new BudgetAlertOptions { NearLimitPercent = 50, OverLimitPercent = 90 });

        Assert.True(service.Evaluate("Food", 100, 60).IsNearLimit);
        Assert.True(service.Evaluate("Food", 100, 95).IsOverLimit);
    }

    [Fact]
    public void Evaluate_CalculatesPercentUsed()
    {
        var status = Create().Evaluate("Rent", 400, 100);

        Assert.Equal(25m, status.PercentUsed);
    }

    [Fact]
    public void BuildAlertMessage_Null_ReturnsNull()
    {
        Assert.Null(Create().BuildAlertMessage(null));
    }

    [Fact]
    public void BuildAlertMessage_UnderLimit_ReturnsNull()
    {
        var service = Create();
        Assert.Null(service.BuildAlertMessage(service.Evaluate("Food", 1000, 100)));
    }

    [Fact]
    public void BuildAlertMessage_OverLimit_MentionsCategory()
    {
        var service = Create();
        var message = service.BuildAlertMessage(service.Evaluate("Food", 1000, 1200));

        Assert.NotNull(message);
        Assert.Contains("Over budget on Food", message);
    }

    [Fact]
    public void BuildAlertMessage_NearLimit_ShowsPercent()
    {
        var service = Create();
        var message = service.BuildAlertMessage(service.Evaluate("Food", 1000, 850));

        Assert.NotNull(message);
        Assert.Contains("Nearing budget on Food", message);
        Assert.Contains("85", message);
    }
}
