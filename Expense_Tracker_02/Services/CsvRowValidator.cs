using System.Globalization;
using Expense_Tracker_02.Models;

namespace Expense_Tracker_02.Services;

/// <summary>Pure validation/mapping of a single CSV row. No file or database access, so it is trivially unit-testable.</summary>
public class CsvRowValidator : ICsvRowValidator
{
    public bool TryValidate(TransactionCsvRow row, int rowNumber, out Transaction? transaction, out string? error)
    {
        transaction = null;

        if (!Enum.TryParse<TransactionType>(row.Type, true, out var type))
        {
            error = $"Row {rowNumber}: invalid Type '{row.Type}' (expected Income/Expense)";
            return false;
        }

        if (!DateTime.TryParse(row.Date, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            error = $"Row {rowNumber}: invalid Date '{row.Date}'";
            return false;
        }

        if (row.Amount <= 0)
        {
            error = $"Row {rowNumber}: Amount must be greater than 0";
            return false;
        }

        if (!TryResolveCategory(row.Category, out var category))
        {
            error = $"Row {rowNumber}: invalid Category '{row.Category}'";
            return false;
        }

        transaction = new Transaction
        {
            Type = type,
            Amount = row.Amount,
            CategoryName = category,
            Date = date.Date,
            Note = row.Note
        };
        error = null;
        return true;
    }

    // Blank -> "Other" (same as before). Unknown names are now rejected instead of silently stored.
    private static bool TryResolveCategory(string? raw, out string category)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            category = "Other";
            return true;
        }

        var match = Category.All.FirstOrDefault(c => string.Equals(c, raw.Trim(), StringComparison.OrdinalIgnoreCase));
        category = match ?? "";
        return match != null;
    }
}
