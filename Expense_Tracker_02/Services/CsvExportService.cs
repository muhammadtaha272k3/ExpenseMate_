using System.Globalization;
using CsvHelper;
using Expense_Tracker_02.Models;

namespace Expense_Tracker_02.Services;

public class CsvExportService : ICsvExportService
{
    public byte[] Export(IEnumerable<Transaction> transactions)
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new StreamWriter(memoryStream, leaveOpen: true))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(transactions.Select(t => new TransactionCsvRow
            {
                Type = t.Type.ToString(),
                Amount = t.Amount,
                Category = t.CategoryName,
                Date = t.Date.ToString("yyyy-MM-dd"),
                Note = t.Note
            }));
        }
        return memoryStream.ToArray();
    }
}
