using FinancialManagerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FinancialManagerAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Revenue> Revenues { get; set; }
        public DbSet<Debt> Debts { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Expense>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Expenses)
                .HasForeignKey(e => e.CategoryId);

            modelBuilder.Entity<Expense>()
                .HasOne(e => e.User)
                .WithMany(u => u.Expenses)
                .HasForeignKey(e => e.UserId);

            modelBuilder.Entity<Revenue>()
                .HasOne(r => r.User)
                .WithMany(u => u.Revenues)
                .HasForeignKey(r => r.UserId);

            modelBuilder.Entity<Debt>()
                .HasOne(d => d.User)
                .WithMany(u => u.Debts)
                .HasForeignKey(d => d.UserId);
        }
    }
}
