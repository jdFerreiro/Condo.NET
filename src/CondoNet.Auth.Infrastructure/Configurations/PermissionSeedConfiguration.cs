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
                new Permission
                {
                    Id = 10100,
                    Name = "ApiKeys",
                    Path = "/apikeys",
                    Description = "Acceso a las claves de API",
                    Image = "apikeys.png"
                },
                new Permission
                {
                    Id = 10200,
                    Name = "Roles",
                    Path = "/roles",
                    Description = "Acceso a los roles definidos",
                    Image = "roles.png"
                },
                new Permission
                {
                    Id = 10300,
                    Name = "Usuarios",
                    Path = "/users",
                    Description = "Acceso a la gestión de usuarios",
                    Image = "users.png"
                },
                new Permission
                {
                    Id = 10400,
                    Name = "Permisos",
                    Path = "/permissions",
                    Description = "Acceso a la gestión de permisos",
                    Image = "permissions.png"
                },
                new Permission
                {
                    Id = 10500,
                    Name = "Auditoría",
                    Path = "/audit",
                    Description = "Acceso a los registros de auditoría",
                    Image = "audit.png"
                },
                new Permission
                {
                    Id = 20001,
                    Name = "Organizaciones",
                    Path = "/organizations",
                    Description = "Ver las organizaciones",
                    Image = "organizations.png"
                },
                new Permission { Id = 2, Name = "Activos", Path = "/assets", Description = "Crear/Editar infraestructura", Image = "assets.png" },
                new Permission { Id = 3, Name = "Estacionamiento", Path = "/parking", Description = "Alquilar puestos a externos", Image = "parking.png" },
                new Permission { Id = 4, Name = "Facturación", Path = "/billing", Description = "Ver estados de cuenta", Image = "billing.png" },
                new Permission { Id = 5, Name = "Residentes", Path = "/residents", Description = "Crear/Editar residentes", Image = "residents.png" }}  
            );
        }
}

}
