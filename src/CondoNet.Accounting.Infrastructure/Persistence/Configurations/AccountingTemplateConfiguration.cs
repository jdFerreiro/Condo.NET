using CondoNet.Accounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CondoNet.Accounting.Infrastructure.Persistence.Configurations
{
    public class AccountingTemplateConfiguration : IEntityTypeConfiguration<AccountingTemplate>
    {
        public void Configure(EntityTypeBuilder<AccountingTemplate> builder)
        {
            builder.ToTable("AccountingTemplates");

            // Llave Primaria
            builder.HasKey(t => t.Id);

            // Propiedades obligatorias y límites de texto
            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(500);

            // Mapeo del Enum como entero en la base de datos
            builder.Property(t => t.EventType)
                .IsRequired()
                .HasConversion<int>();

            // 🔍 ÍNDICE CRÍTICO MULTI-TENANT: Un condominio solo puede tener UNA plantilla por evento de negocio
            builder.HasIndex(t => new { t.CondominiumId, t.EventType })
                .IsUnique()
                .HasDatabaseName("IX_AccountingTemplates_Condo_EventType");

            // Índice auxiliar para búsquedas rápidas por organización
            builder.HasIndex(t => t.OrganizationId);

            // Relación 1:N con sus reglas hijas
            builder.HasMany(t => t.Rules)
                .WithOne(r => r.Template)
                .HasForeignKey(r => r.AccountingTemplateId)
                .OnDelete(DeleteBehavior.Cascade); // Borrado en cascada: Si se elimina la plantilla, se van sus reglas
        }
    }
}
