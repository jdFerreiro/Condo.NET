using CondoNet.Auth.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Auth.Infrastructure.Persistence;

public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>(); // Usamos la clase explícita
    public DbSet<User> Users => Set<User>();
    public DbSet<UserContext> UserContexts => Set<UserContext>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.ToTable("apiKeys");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired();
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.OrganizationId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.ToTable("passwordResetTokens");
            entity.Property(e => e.Token).IsRequired();
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ExpiresAt).IsRequired();
            entity.Property(e => e.IsUsed).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.User)
                .WithMany(p => p.PasswordResetTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        });


        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("permissions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);

            entity.HasMany(rp => rp.RolePermissions)
                .WithOne(r => r.Permission)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refreshTokens");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired();
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ExpiresAt).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.User)
                .WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();

            entity.HasMany(rp => rp.RolePermissions)
                .WithOne(p => p.Role)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(uc => uc.Contexts)
                .WithOne(c => c.Role)
                .HasForeignKey(c => c.RoleId)
                .OnDelete(DeleteBehavior.SetNull);
        });


        // 2. Roles y Permisos (Muchos a Muchos con clase explícita)
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("rolePermissions");
            entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });

            entity.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            entity.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);
        });

        // 1. Usuarios
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // 3. Contextos de Usuario
        modelBuilder.Entity<UserContext>(entity =>
        {
            entity.ToTable("userContexts");
            entity.HasIndex(e => new { e.OrganizationId, e.CondoId });

            entity.HasOne(d => d.User)
                .WithMany(p => p.Contexts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Role)
                .WithMany(p => p.Contexts)
                .HasForeignKey(d => d.RoleId);
        });


        // 5. Aplicar configuraciones externas (Seed Data)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
    }
}
