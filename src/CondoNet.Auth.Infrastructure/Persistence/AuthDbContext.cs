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
            // 1. Nombre de tabla y llave primaria
            entity.ToTable("permissions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1500);
            entity.Property(e => e.Path).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Icon).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DisplayOrder).IsRequired();

            // 3. Configuración de la Relación Jerárquica Recursiva (Árbol multinivel de Menús)
            entity.HasOne(e => e.ParentPermission)
                .WithMany(p => p.ChildPermissions)
                .HasForeignKey(e => e.ParentPermissionId)
                .OnDelete(DeleteBehavior.Restrict); // Evita borrar un menú padre si todavía tiene submenús asociados

            // 4. Relación Muchos a Muchos (M:N) con Roles a través de la tabla intermedia
            entity.HasMany(rp => rp.RolePermissions)
                .WithOne(r => r.Permission)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade); // Si se elimina el permiso, se limpian sus asignaciones en los roles
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

        });

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

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<UserContext>(entity =>
        {
            entity.ToTable("userContexts");
            entity.HasIndex(e => new { e.OrganizationId, e.CondoId });

            entity.HasOne(d => d.User)
                .WithMany(p => p.Contexts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(d => d.Roles)
                .WithMany(p => p.Contexts)
                .UsingEntity(j => j.ToTable("userContextRoles"));
        });

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
    }
}
