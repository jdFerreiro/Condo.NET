using CondoNet.Auth.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CondoNet.Auth.Infrastructure.Configurations;

public class UserContextSeedConfiguration : IEntityTypeConfiguration<UserContext>
{
    public void Configure(EntityTypeBuilder<UserContext> builder)
    {
        builder.HasData(new UserContext
        {
            Id = Guid.Parse("f1f1f1f1-f1f1-f1f1-f1f1-f1f1f1f1f1f1"),
            UserId = Guid.Parse("d3d3d3d3-d3d3-d3d3-d3d3-d3d3d3d3d3d3"),
            OrganizationId = Guid.Parse("a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1"),
            CondoId = Guid.Parse("c2c2c2c2-c2c2-c2c2-c2c2-c2c2c2c2c2c2"),
            RoleId = 1
        });
    }
}
