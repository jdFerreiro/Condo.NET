using CondoNet.Auth.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CondoNet.Auth.Infrastructure.Configurations;

public class RoleSeedConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasData(
            new Role { Id = 1, Name = "SUPERADMIN" },
            new Role { Id = 2, Name = "ADMIN" },
            new Role { Id = 3, Name = "PROPIETARIO" },
            new Role { Id = 4, Name = "INQUILINO" },
            new Role { Id = 5, Name = "EXTERNO" },
            new Role { Id = 6, Name = "CONSERJE" },
            new Role { Id = 7, Name = "JUNTA DE CONDOMINIO" },
            new Role { Id = 8, Name = "PROVEEDOR" },
            new Role { Id = 9, Name = "ADMINISTRADOR DE PROPIEDAD" },
            new Role { Id = 10, Name = "ADMINISTRADOR DE ESTACIONAMIENTO" },
            new Role { Id = 11, Name = "AUDITO" } // Un rol de solo lectura (generalmente para contadores externos o comisiones revisoras) que necesita ver finanzas (financialRoute / accountingRoute) pero no debe poder modificar saldos ni transacciones.
        );
    }
}
