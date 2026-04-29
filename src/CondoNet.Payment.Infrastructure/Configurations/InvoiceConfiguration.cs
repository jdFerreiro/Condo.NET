using CondoNet.Payment.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CondoNet.Payment.Infrastructure.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.ToTable("Invoices");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Number).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Amount).IsRequired();
            builder.Property(x => x.IssuedAt).IsRequired();
        }
    }
}