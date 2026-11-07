using System.Text;
using Expense_Tracker_02.Services;
using Xunit;

namespace ExpenseMate.Tests.Unit;

public class CsvImportServiceTests
{
    private readonly CsvImportService _service = new(new CsvRowValidator());

    private static MemoryStream Csv(string content) => new(Encoding.UTF8.GetBytes(content));

    [Fact]
    public void Import_SkipsBadRows_AndKeepsGoodOnes()
    {
        var csv = "Type,Amount,Category,Date,Note\n" +
                  "Expense,25.50,Food,2026-11-01,lunch\n" +
                  "Gift,10,Food,2026-11-02,bad type\n" +
                  "Income,1000,Salary,2026-11-03,pay\n";

        var result = _service.Import(Csv(csv));

        Assert.Equal(2, result.Valid.Count);
        Assert.Single(result.Errors);
        Assert.Contains("Row 3", result.Errors[0]);
    }

    [Fact]
    public void Import_UnreadableAmount_ReportsRowError()
    {
        var csv = "Type,Amount,Category,Date,Note\n" +
                  "Expense,abc,Food,2026-11-01,x\n";

        var result = _service.Import(Csv(csv));

        Assert.Empty(result.Valid);
        Assert.Single(result.Errors);
        Assert.Contains("Row 2", result.Errors[0]);
    }

    [Fact]
    public void Import_EmptyFile_ReturnsNothing()
    {
        var result = _service.Import(Csv(""));

        Assert.Empty(result.Valid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ExportThenImport_RoundTripsData()
    {
        var export = new CsvExportService();
        var original = new List<Expense_Tracker_02.Models.Transaction>
        {
            new() { Type = Expense_Tracker_02.Models.TransactionType.Expense, Amount = 12.34m,
                    CategoryName = "Transport", Date = new DateTime(2026, 10, 5), Note = "bus" }
        };

        var bytes = export.Export(original);
        var result = _service.Import(new MemoryStream(bytes));

        Assert.Empty(result.Errors);
        var t = Assert.Single(result.Valid);
        Assert.Equal(12.34m, t.Amount);
        Assert.Equal("Transport", t.CategoryName);
        Assert.Equal(new DateTime(2026, 10, 5), t.Date);
    }
}
