using CondoNet.Payment.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CondoNet.Payment.Infrastructure.Configurations
{
    public class PaymentReceiptConfiguration : IEntityTypeConfiguration<PaymentReceipt>
    {
        public void Configure(EntityTypeBuilder<PaymentReceipt> builder)
        {
            builder.ToTable("PaymentReceipts");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ReceiptNumber).HasMaxLength(100).IsRequired();
            builder.Property(x => x.IssuedAt).IsRequired();
        }
    }
}