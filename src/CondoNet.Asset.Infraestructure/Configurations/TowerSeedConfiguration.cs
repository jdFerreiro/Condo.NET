using CondoNet.Asset.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CondoNet.Asset.Infrastructure.Configurations
{
    public class TowerSeedConfiguration : IEntityTypeConfiguration<Tower>
    {
        public void Configure(EntityTypeBuilder<Tower> builder)
        {
            builder.HasData(
                new Tower
                {
                    Id = SeedDataConstants.TowerAId,
                    OrganizationId = SeedDataConstants.TestOrganizationId,
                    CondominiumId = SeedDataConstants.TestCondoId,
                    Name = "Torre A"
                },
                new Tower
                {
                    Id = SeedDataConstants.TowerBId,
                    OrganizationId = SeedDataConstants.TestOrganizationId,
                    CondominiumId = SeedDataConstants.TestCondoId,
                    Name = "Torre B"
                }

            );
        }
    }
}
