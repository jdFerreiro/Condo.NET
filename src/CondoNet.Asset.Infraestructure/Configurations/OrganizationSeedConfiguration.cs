using CondoNet.Asset.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CondoNet.Asset.Infraestructure.Configurations
{
    public class OrganizationSeedConfiguration : IEntityTypeConfiguration<Organization>
    {
        public void Configure(EntityTypeBuilder<Organization> builder)
        {
            builder.HasData(
                new Organization
                {
                    Id = SeedDataConstants.TestOrganizationId,
                    Name = "Administradora Global CondoNet",
                    TaxId = "J-12345678-0",
                    BaseCurrency = "USD",
                    ContactEmail = "admin@condonet.test",
                    CreatedAt = SeedDataConstants.TestOrganizationCreatedAt
                }
            );
        }
    }
}
