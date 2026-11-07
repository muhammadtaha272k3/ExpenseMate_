using Expense_Tracker_02.Models;
using System.Formats.Asn1;  
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace Expense_Tracker_02.Services
{
    public class TransactionCsvRow
    {
        public string Type { get; set; } = "";
        public decimal Amount { get; set; }
        public string Category { get; set; } = "";
        public string Date { get; set; } = "";
        public string? Note { get; set; }
    }

    public class CsvService
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

        // Returns parsed transactions plus row-level error messages so the
        // controller can report exactly which rows failed, instead of the whole
        // import failing on one bad row.
        public (List<Transaction> Valid, List<string> Errors) Import(Stream fileStream)
        {
            var valid = new List<Transaction>();
            var errors = new List<string>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { HeaderValidated = null, MissingFieldFound = null };
            using var reader = new StreamReader(fileStream);
            using var csv = new CsvReader(reader, config);

            csv.Read();
            csv.ReadHeader();
            int rowNum = 1;
            while (csv.Read())
            {
                rowNum++;
                try
                {
                    var row = csv.GetRecord<TransactionCsvRow>();
                    if (!Enum.TryParse<TransactionType>(row.Type, true, out var type))
                    {
                        errors.Add($"Row {rowNum}: invalid Type '{row.Type}' (expected Income/Expense)");
                        continue;
                    }
                    if (!DateTime.TryParse(row.Date, out var date))
                    {
                        errors.Add($"Row {rowNum}: invalid Date '{row.Date}'");
                        continue;
                    }
                    if (row.Amount <= 0)
                    {
                        errors.Add($"Row {rowNum}: Amount must be greater than 0");
                        continue;
                    }

                    valid.Add(new Transaction
                    {
                        Type = type,
                        Amount = row.Amount,
                        CategoryName = string.IsNullOrWhiteSpace(row.Category) ? "Other" : row.Category,
                        Date = date,
                        Note = row.Note
                    });
                }
                catch (Exception ex)
                {
                    errors.Add($"Row {rowNum}: {ex.Message}");
                }
            }

            return (valid, errors);
        }
    }
}
