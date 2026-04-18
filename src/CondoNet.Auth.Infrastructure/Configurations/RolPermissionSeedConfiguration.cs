using CondoNet.Auth.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CondoNet.Auth.Infrastructure.Configurations
{
    public class RolPermissionSeedConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.HasData(
                new { RoleId = 1, PermissionId = 1 },
                new { RoleId = 1, PermissionId = 2 },
                new { RoleId = 1, PermissionId = 3 },
                new { RoleId = 1, PermissionId = 4 },
                new { RoleId = 1, PermissionId = 5 },
                new { RoleId = 2, PermissionId = 3 },
                new { RoleId = 2, PermissionId = 4 },
                new { RoleId = 2, PermissionId = 5 }
            );
        }
    }
}
