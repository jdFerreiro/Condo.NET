using CondoNet.Payment.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CondoNet.Payment.Infrastructure.Configurations
{
    public class WebhookEventConfiguration : IEntityTypeConfiguration<WebhookEvent>
    {
        public void Configure(EntityTypeBuilder<WebhookEvent> builder)
        {
            builder.ToTable("WebhookEvents");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.EventType).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Payload).IsRequired();
            builder.Property(x => x.ReceivedAt).IsRequired();
        }
    }
}