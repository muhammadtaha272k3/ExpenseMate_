using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace Expense_Tracker_02.Services;

/// <summary>Reads CSV rows and delegates per-row validation. One bad row never fails the whole import.</summary>
public class CsvImportService : ICsvImportService
{
    private readonly ICsvRowValidator _validator;

    public CsvImportService(ICsvRowValidator validator) => _validator = validator;

    public CsvImportResult Import(Stream fileStream)
    {
        var result = new CsvImportResult(new(), new());

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        };

        using var reader = new StreamReader(fileStream);
        using var csv = new CsvReader(reader, config);

        if (!csv.Read() || !csv.ReadHeader()) return result; // empty file

        int rowNumber = 1;
        while (csv.Read())
        {
            rowNumber++;
            try
            {
                var row = csv.GetRecord<TransactionCsvRow>();
                if (_validator.TryValidate(row, rowNumber, out var transaction, out var error))
                    result.Valid.Add(transaction!);
                else
                    result.Errors.Add(error!);
            }
            catch (CsvHelperException ex)
            {
                result.Errors.Add($"Row {rowNumber}: could not read row ({ex.Message.Split('\n')[0]})");
            }
        }

        return result;
    }
}
