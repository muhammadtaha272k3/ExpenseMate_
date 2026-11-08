using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Expense_Tracker_02.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Date_CategoryName",
                table: "Transactions",
                columns: new[] { "Date", "CategoryName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_Date_CategoryName",
                table: "Transactions");
        }
    }
}
