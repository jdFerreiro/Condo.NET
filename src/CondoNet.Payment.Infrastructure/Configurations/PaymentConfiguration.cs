using CondoNet.Payment.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CondoNet.Payment.Infrastructure.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Core.Entities.Payment>
    {
        public void Configure(EntityTypeBuilder<Core.Entities.Payment> builder)
        {
            builder.ToTable("Payments");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Amount).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
            builder.HasOne(x => x.Invoice)
                .WithMany(i => i.Payments)
                .HasForeignKey(x => x.InvoiceId);
            builder.HasOne(x => x.PaymentGateway)
                .WithMany(pg => pg.Payments)
                .HasForeignKey(x => x.PaymentGatewayId);
            builder.HasOne(x => x.Receipt)
                .WithOne(r => r.Payment)
                .HasForeignKey<PaymentReceipt>(r => r.PaymentId);
        }
    }
}