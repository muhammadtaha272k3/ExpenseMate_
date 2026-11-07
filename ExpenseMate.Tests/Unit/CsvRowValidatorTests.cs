using Expense_Tracker_02.Models;
using Expense_Tracker_02.Services;
using Xunit;

namespace ExpenseMate.Tests.Unit;

public class CsvRowValidatorTests
{
    private readonly CsvRowValidator _validator = new();

    private static TransactionCsvRow Row(
        string type = "Expense", decimal amount = 10m, string category = "Food", string date = "2026-11-01") =>
        new() { Type = type, Amount = amount, Category = category, Date = date, Note = "n" };

    [Fact]
    public void ValidRow_ReturnsTransaction()
    {
        var ok = _validator.TryValidate(Row(), 2, out var t, out var error);

        Assert.True(ok);
        Assert.Null(error);
        Assert.Equal(TransactionType.Expense, t!.Type);
        Assert.Equal(10m, t.Amount);
        Assert.Equal("Food", t.CategoryName);
        Assert.Equal(new DateTime(2026, 11, 1), t.Date);
    }

    [Fact]
    public void Type_IsCaseInsensitive()
    {
        Assert.True(_validator.TryValidate(Row(type: "income"), 2, out var t, out _));
        Assert.Equal(TransactionType.Income, t!.Type);
    }

    [Fact]
    public void InvalidType_Fails()
    {
        Assert.False(_validator.TryValidate(Row(type: "Gift"), 3, out _, out var error));
        Assert.Contains("Row 3", error);
        Assert.Contains("Type", error);
    }

    [Fact]
    public void InvalidDate_Fails()
    {
        Assert.False(_validator.TryValidate(Row(date: "not-a-date"), 4, out _, out var error));
        Assert.Contains("Date", error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void NonPositiveAmount_Fails(int amount)
    {
        Assert.False(_validator.TryValidate(Row(amount: amount), 5, out _, out var error));
        Assert.Contains("Amount", error);
    }

    [Fact]
    public void BlankCategory_DefaultsToOther()
    {
        Assert.True(_validator.TryValidate(Row(category: "  "), 2, out var t, out _));
        Assert.Equal("Other", t!.CategoryName);
    }

    [Fact]
    public void KnownCategory_IsNormalisedToCanonicalCase()
    {
        Assert.True(_validator.TryValidate(Row(category: "food"), 2, out var t, out _));
        Assert.Equal("Food", t!.CategoryName);
    }

    [Fact]
    public void UnknownCategory_Fails()
    {
        Assert.False(_validator.TryValidate(Row(category: "Spaceships"), 6, out _, out var error));
        Assert.Contains("Category", error);
    }
}
