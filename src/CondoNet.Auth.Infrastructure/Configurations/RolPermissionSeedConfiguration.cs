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
                new { RoleId = 1, PermissionId = 10000 },
                new { RoleId = 1, PermissionId = 20000 },
                new { RoleId = 1, PermissionId = 20100 },
                new { RoleId = 1, PermissionId = 20200 },
                new { RoleId = 1, PermissionId = 20300 },
                new { RoleId = 1, PermissionId = 20400 },
                new { RoleId = 1, PermissionId = 20500 },
                new { RoleId = 1, PermissionId = 30000 },
                new { RoleId = 1, PermissionId = 30100 },
                new { RoleId = 1, PermissionId = 30200 },
                new { RoleId = 1, PermissionId = 30300 },
                new { RoleId = 1, PermissionId = 40000 },
                new { RoleId = 1, PermissionId = 40100 },
                new { RoleId = 1, PermissionId = 40200 },
                new { RoleId = 1, PermissionId = 50000 },
                new { RoleId = 1, PermissionId = 50100 },
                new { RoleId = 1, PermissionId = 50200 },
                new { RoleId = 1, PermissionId = 50300 },
                new { RoleId = 1, PermissionId = 50400 },
                new { RoleId = 1, PermissionId = 60000 },
                new { RoleId = 1, PermissionId = 60100 },
                new { RoleId = 1, PermissionId = 60200 },
                new { RoleId = 1, PermissionId = 60300 },
                new { RoleId = 1, PermissionId = 60400 },
                new { RoleId = 1, PermissionId = 60500 },
                new { RoleId = 1, PermissionId = 90000 },
                new { RoleId = 1, PermissionId = 90100 },
                new { RoleId = 1, PermissionId = 90200 },
                new { RoleId = 1, PermissionId = 90300 },
                new { RoleId = 1, PermissionId = 90400 },
                new { RoleId = 1, PermissionId = 90500 }
            );
        }
    }
}
