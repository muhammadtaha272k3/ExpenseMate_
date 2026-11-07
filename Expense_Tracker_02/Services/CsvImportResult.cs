using Expense_Tracker_02.Models;

namespace Expense_Tracker_02.Services;

public record CsvImportResult(List<Transaction> Valid, List<string> Errors);
