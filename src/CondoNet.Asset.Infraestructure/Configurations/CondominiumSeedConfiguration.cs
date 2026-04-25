using CondoNet.Asset.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CondoNet.Asset.Infrastructure.Configurations
{
    public class CondominiumSeedConfiguration : IEntityTypeConfiguration<Condominium>
    {
        public void Configure(EntityTypeBuilder<Condominium> builder)
        {
            builder.HasData(
                new Condominium
                {
                    Id = SeedDataConstants.TestCondoId,
                    OrganizationId = SeedDataConstants.TestOrganizationId, // Obligatorio
                    Name = "Residencias Sol y Mar",
                    TaxId = "J-87654321-0",
                    Address = "Av. Principal, Edificio Sol y Mar",
                    ReserveFundPercentage = 10.00m
                }
            );
        }
    }
}
