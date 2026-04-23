using CondoNet.Asset.Core.Entities;
using CondoNet.Shared.Asset;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CondoNet.Asset.Infraestructure.Configurations
{
    public class UnitSeedConfiguration : IEntityTypeConfiguration<Unit>
    {
        public void Configure(EntityTypeBuilder<Unit> builder)
        {
            var TAU1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var TAU2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var TBU1 = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var TBU2 = Guid.Parse("44444444-4444-4444-4444-444444444444");

            builder.HasData(
            new Unit
            {
                Id = TAU1,
                OrganizationId = SeedDataConstants.TestOrganizationId,
                TowerId = SeedDataConstants.TowerAId,
                Identifier = "A01",
                Floor = "1",
                Aliquot = 50.0000m,
                Type = UnitType.Habitational,
                OwnerEmail = "vecino101@test.com"
            },
            new Unit
            {
                Id = TAU2,
                OrganizationId = SeedDataConstants.TestOrganizationId,
                TowerId = SeedDataConstants.TowerAId,
                Identifier = "A02",
                Floor = "1",
                Aliquot = 50.0000m,
                Type = UnitType.Habitational,
                OwnerEmail = "vecino102@test.com"
            },
            new Unit
            {
                Id = TBU1,
                OrganizationId = SeedDataConstants.TestOrganizationId,
                TowerId = SeedDataConstants.TowerAId,
                Identifier = "B01",
                Floor = "1",
                Aliquot = 50.0000m,
                Type = UnitType.Habitational,
                OwnerEmail = "vecino101@test.com"
            },
            new Unit
            {
                Id = TBU2,
                OrganizationId = SeedDataConstants.TestOrganizationId,
                TowerId = SeedDataConstants.TowerAId,
                Identifier = "B02",
                Floor = "1",
                Aliquot = 50.0000m,
                Type = UnitType.Habitational,
                OwnerEmail = "vecino102@test.com"
            });
        }
    }
}
