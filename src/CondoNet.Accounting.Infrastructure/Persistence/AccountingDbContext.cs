using CondoNet.Accounting.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Accounting.Infrastructure.Persistence;

public class AccountingDbContext : DbContext
{
    public AccountingDbContext(DbContextOptions<AccountingDbContext> options) : base(options) { }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<AccountingTransaction> Transactions { get; set; }
    public DbSet<AccountingEntry> Entries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Relación: Transaction -> Entries
        modelBuilder.Entity<AccountingTransaction>()
            .HasMany(t => t.Entries)
            .WithOne(e => e.Transaction)
            .HasForeignKey(e => e.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación: Account -> Entries
        modelBuilder.Entity<Account>()
            .HasMany(a => a.Entries)
            .WithOne(e => e.Account)
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
