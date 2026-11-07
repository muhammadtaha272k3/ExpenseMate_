using Expense_Tracker_02.Data;
using Expense_Tracker_02.Repositories;
using Expense_Tracker_02.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=expensemate.db"));

// Repository layer (the only code that touches ApplicationDbContext)
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();

// Service layer (business rules), registered by interface
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<IBudgetAlertService, BudgetAlertService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ICsvRowValidator, CsvRowValidator>();
builder.Services.AddScoped<ICsvImportService, CsvImportService>();
builder.Services.AddScoped<ICsvExportService, CsvExportService>();

builder.Services.Configure<BudgetAlertOptions>(builder.Configuration.GetSection("BudgetAlerts"));

var app = builder.Build();

CurrencyHelper.Symbol = builder.Configuration["CurrencySettings:Symbol"] ?? "$";

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // Keep EnsureCreated() until you finish the migration baseline (see README step 6),
    // then replace it with db.Database.Migrate().
    db.Database.EnsureCreated();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Lets the test project start the app with WebApplicationFactory<Program>.
public partial class Program { }
