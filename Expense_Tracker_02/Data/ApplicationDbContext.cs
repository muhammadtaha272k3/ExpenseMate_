using Expense_Tracker_02.Models;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker_02.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<Budget> Budgets => Set<Budget>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
            modelBuilder.Entity<Budget>()
                .HasIndex(b => new { b.CategoryName, b.Month, b.Year })
                .IsUnique();
        }
    }
}
