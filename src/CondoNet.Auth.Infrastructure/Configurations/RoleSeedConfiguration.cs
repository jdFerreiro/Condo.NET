using CondoNet.Auth.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CondoNet.Auth.Infrastructure.Configurations;

public class RoleSeedConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasData(
            new Role { Id = 1, Name = "ADMIN" },
            new Role { Id = 2, Name = "OWNER" },
            new Role { Id = 3, Name = "RESIDENT" },
            new Role { Id = 4, Name = "EXTERNAL" },
            new Role { Id = 5, Name = "CONCIERGE" },
            new Role { Id = 6, Name = "JUNTA DE CONDOMINIO" },
            new Role { Id = 7, Name = "PROVEEDOR" },
            new Role { Id = 8, Name = "ADMINISTRADOR DE PROPIEDAD" },
            new Role { Id = 9, Name = "ADMINISTRADOR DE ESTACIONAMIENTO" }
        );
    }
}
