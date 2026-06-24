using CondoNet.Accounting.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CondoNet.Accounting.Infrastructure.Persistence.Configurations
{
    public class TemplateRuleConfiguration : IEntityTypeConfiguration<TemplateRule>
    {
        public void Configure(EntityTypeBuilder<TemplateRule> builder)
        {
            builder.ToTable("TemplateRules");

            // Llave Primaria
            builder.HasKey(r => r.Id);

            // Mapeo del Enum de movimiento (Debit / Credit) como entero
            builder.Property(r => r.Movement)
                .IsRequired()
                .HasConversion<int>();

            // Configuración quirúrgica de precisión decimal para los factores porcentuales (Ej: 0.1600)
            builder.Property(r => r.PercentageFactor)
                .IsRequired()
                .HasPrecision(18, 4); // 18 dígitos en total, 4 decimales de precisión contable

            // Relación externa con la cuenta contable (Regla de negocio)
            // No configuramos la navegación física si 'Account' está en otra tabla, pero definimos el índice
            builder.HasIndex(r => r.AccountId);

            // Índice para optimizar los joins de carga de la plantilla
            builder.HasIndex(r => r.AccountingTemplateId);
        }
    }
}
