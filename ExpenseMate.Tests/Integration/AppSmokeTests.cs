using System.Net;
using Expense_Tracker_02.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ExpenseMate.Tests.Integration;

/// <summary>Boots the real app (all layers + Razor views) on in-memory SQLite and requests each main page.</summary>
public class TestWebAppFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public TestWebAppFactory() => _connection.Open();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var toRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                (d.ServiceType.FullName?.Contains("IDbContextOptionsConfiguration") ?? false)).ToList();
            foreach (var d in toRemove) services.Remove(d);

            services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }
}

public class AppSmokeTests : IClassFixture<TestWebAppFactory>
{
    private readonly TestWebAppFactory _factory;

    public AppSmokeTests(TestWebAppFactory factory) => _factory = factory;

    [Theory]
    [InlineData("/")]
    [InlineData("/Transactions")]
    [InlineData("/Transactions/Create")]
    [InlineData("/Transactions/ImportCsv")]
    [InlineData("/Budgets")]
    [InlineData("/Budgets/Create")]
    [InlineData("/Reports")]
    public async Task Page_LoadsSuccessfully(string url)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ExportCsv_ReturnsCsvFile()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/Transactions/ExportCsv");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
    }
}
