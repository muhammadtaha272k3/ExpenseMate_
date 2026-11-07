namespace Expense_Tracker_02.Services;

public interface ICsvImportService
{
    CsvImportResult Import(Stream fileStream);
}
