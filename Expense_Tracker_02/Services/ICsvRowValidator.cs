using Expense_Tracker_02.Models;

namespace Expense_Tracker_02.Services;

public interface ICsvRowValidator
{
    /// <summary>Validates one CSV row. Returns true and a Transaction, or false and an error message.</summary>
    bool TryValidate(TransactionCsvRow row, int rowNumber, out Transaction? transaction, out string? error);
}
