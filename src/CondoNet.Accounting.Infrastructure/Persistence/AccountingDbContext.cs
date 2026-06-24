using CondoNet.Accounting.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Accounting.Infrastructure.Persistence
{
    public class AccountingDbContext(DbContextOptions<AccountingDbContext> options) : DbContext(options)
    {

        // DbSets Completos del Microservicio Contable Avanzado
        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<AccountingTransaction> Transactions { get; set; } = null!;
        public DbSet<AccountingEntry> Entries { get; set; } = null!; // <- LA ENTIDAD QUE FALTABA
        public DbSet<AccountingTemplate> Templates { get; set; } = null!;
        public DbSet<TemplateRule> TemplateRules { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountingDbContext).Assembly);
        }
    }
}
