using CondoNet.Auth.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CondoNet.Auth.Infrastructure.Configurations;

public class UserSeedConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        var adminId = Guid.Parse("d3d3d3d3-d3d3-d3d3-d3d3-d3d3d3d3d3d3");

        builder.HasData(new User
        {
            Id = adminId,
            Email = "admin@condo.net",
            FullName = "Admin Maestro",
            IsActive = true,
            // Password: Condo123!
            PasswordHash = "$2a$11$UJFPSGUU2mYbVcEDyWSPqOLRX8CM8qoWnMhKBGlX6y2xAdGy6YuLi",
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
