using CondoNet.Auth.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CondoNet.Auth.Infrastructure.Configurations;

public class ApiKeySeedConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        // var keyHashed = BC.HashPassword("AdminKey");
        var adminApiKeyId = Guid.Parse("e4e4e4e4-e4e4-e4e4-e4e4-e4e4e4e4e4e4");
        var keyHashed = "$2a$11$v98PIDTJnp4O33B3fkBziOubl6Lup5ccvDhuZHAziRorTxlJXikGO"; // Aquí deberías colocar el hash de la llave "AdminKey" generado previamente

        builder.HasData(new ApiKey
        {
            Id = adminApiKeyId,
            // Esta es la llave que pondremos en Postman
            // Key: "AdminKey"
            Key = keyHashed,
            OrganizationId = Guid.Parse("a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1"),
            Description = "Llave de desarrollo para Postman",
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
