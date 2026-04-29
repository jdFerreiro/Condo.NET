using CondoNet.Payment.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CondoNet.Payment.Infrastructure.Configurations
{
    public class PaymentGatewayConfiguration : IEntityTypeConfiguration<PaymentGateway>
    {
        public void Configure(EntityTypeBuilder<PaymentGateway> builder)
        {
            builder.ToTable("PaymentGateways");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Provider).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Configuration).IsRequired();
        }
    }
}