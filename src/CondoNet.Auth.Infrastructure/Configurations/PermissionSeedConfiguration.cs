using CondoNet.Auth.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CondoNet.Auth.Infrastructure.Configurations
{

    public class PermissionSeedConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.HasData(
                new Permission { Id = 1, Name = "assets:view", Description = "Ver torres y unidades" },
                new Permission { Id = 2, Name = "assets:manage", Description = "Crear/Editar infraestructura" },
                new Permission { Id = 3, Name = "parking:lease", Description = "Alquilar puestos a externos" },
                new Permission { Id = 4, Name = "billing:view", Description = "Ver estados de cuenta" },
                new Permission { Id = 5, Name = "resident:manage", Description = "Crear/Editar residentes" }
            );
        }
    }

}
