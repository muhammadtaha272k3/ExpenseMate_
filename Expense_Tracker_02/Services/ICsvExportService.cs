using Expense_Tracker_02.Models;

namespace Expense_Tracker_02.Services;

public interface ICsvExportService
{
    byte[] Export(IEnumerable<Transaction> transactions);
}
