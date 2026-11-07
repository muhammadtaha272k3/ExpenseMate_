using Expense_Tracker_02.Data;
using Expense_Tracker_02.Repositories;
using Expense_Tracker_02.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ExpenseMate.Tests.Integration;

/// <summary>
/// Real chain for integration tests: Service -> Repository -> EF Core -> in-memory SQLite.
/// Nothing is mocked below the service layer.
/// </summary>
public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection;

    public ApplicationDbContext Db { get; }
    public ITransactionService Transactions { get; }
    public IBudgetService Budgets { get; }
    public IReportService Reports { get; }

    public TestDb()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options;
        Db = new ApplicationDbContext(options);
        Db.Database.EnsureCreated();

        var transactionRepo = new TransactionRepository(Db);
        var budgetRepo = new BudgetRepository(Db);
        var alerts = new BudgetAlertService(budgetRepo, transactionRepo, Options.Create(new BudgetAlertOptions()));

        Transactions = new TransactionService(
            transactionRepo, alerts, new CsvExportService(), new CsvImportService(new CsvRowValidator()));
        Budgets = new BudgetService(budgetRepo, alerts);
        Reports = new ReportService(transactionRepo);
    }

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
    }
}
