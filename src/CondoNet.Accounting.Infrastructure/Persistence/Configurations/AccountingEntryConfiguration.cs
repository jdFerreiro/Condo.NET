using CondoNet.Accounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CondoNet.Accounting.Infrastructure.Persistence.Configurations
{
    public class AccountingEntryConfiguration : IEntityTypeConfiguration<AccountingEntry>
    {
        public void Configure(EntityTypeBuilder<AccountingEntry> builder)
        {
            builder.ToTable("AccountingEntries");

            // 1. Llave Primaria
            builder.HasKey(e => e.Id);

            // 2. Precisión Financiera Estricta (Blindaje contra pérdida de centavos)
            builder.Property(e => e.Debit)
                .IsRequired()
                .HasPrecision(18, 4); // 18 dígitos en total, 4 decimales estándar contable

            builder.Property(e => e.Credit)
                .IsRequired()
                .HasPrecision(18, 4);

            // 3. Notas Auxiliares Opcionales (Glosas por renglón)
            builder.Property(e => e.Reference)
                .HasMaxLength(250)
                .IsRequired(false);

            // 4. Índices Críticos Multi-Tenant y de Búsqueda para Grillas del Libro Diario
            builder.HasIndex(e => e.CondominiumId);
            builder.HasIndex(e => e.OrganizationId);
            builder.HasIndex(e => e.AccountId);
            builder.HasIndex(e => e.AccountingTransactionId);

            // 5. Relaciones y Comportamiento de Borrado (Cascade Delete)
            // Cada renglón pertenece obligatoriamente a una transacción cabecera
            builder.HasOne(e => e.Transaction)
                .WithMany(t => t.Entries)
                .HasForeignKey(e => e.AccountingTransactionId)
                .OnDelete(DeleteBehavior.Cascade); // Si se elimina la transacción, sus renglones se borran automáticamente

            // Relación con la cuenta contable que recibe el movimiento
            builder.HasOne(e => e.Account)
                .WithMany() // Una cuenta puede estar en N renglones a lo largo del año
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Restrict); // Regla de Oro: Prohibido borrar una cuenta si tiene movimientos contables
        }
    }
}
