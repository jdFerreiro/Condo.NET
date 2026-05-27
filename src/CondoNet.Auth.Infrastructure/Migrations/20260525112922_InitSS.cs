using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CondoNet.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitSS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "apiKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apiKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "rolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_rolePermissions_permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rolePermissions_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "passwordResetTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", maxLength: 50, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_passwordResetTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_passwordResetTokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "refreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", maxLength: 50, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_refreshTokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "userContexts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CondoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userContexts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_userContexts_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "userContextRoles",
                columns: table => new
                {
                    ContextsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RolesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userContextRoles", x => new { x.ContextsId, x.RolesId });
                    table.ForeignKey(
                        name: "FK_userContextRoles_roles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_userContextRoles_userContexts_ContextsId",
                        column: x => x.ContextsId,
                        principalTable: "userContexts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "apiKeys",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Key", "OrganizationId" },
                values: new object[] { new Guid("e4e4e4e4-e4e4-e4e4-e4e4-e4e4e4e4e4e4"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Llave de desarrollo para Postman", true, "$2a$11$v98PIDTJnp4O33B3fkBziOubl6Lup5ccvDhuZHAziRorTxlJXikGO", new Guid("a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1") });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Ver torres y unidades", "assets:view" },
                    { 2, "Crear/Editar infraestructura", "assets:manage" },
                    { 3, "Alquilar puestos a externos", "parking:lease" },
                    { 4, "Ver estados de cuenta", "billing:view" },
                    { 5, "Crear/Editar residentes", "resident:manage" }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "ADMIN" },
                    { 2, "OWNER" },
                    { 3, "RESIDENT" },
                    { 4, "EXTERNAL" },
                    { 5, "CONCIERGE" },
                    { 6, "JUNTA DE CONDOMINIO" },
                    { 7, "PROVEEDOR" },
                    { 8, "ADMINISTRADOR DE PROPIEDAD" },
                    { 9, "ADMINISTRADOR DE ESTACIONAMIENTO" }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash" },
                values: new object[] { new Guid("d3d3d3d3-d3d3-d3d3-d3d3-d3d3d3d3d3d3"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@condo.net", "Admin Maestro", true, "$2a$11$UJFPSGUU2mYbVcEDyWSPqOLRX8CM8qoWnMhKBGlX6y2xAdGy6YuLi" });

            migrationBuilder.InsertData(
                table: "rolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 1 },
                    { 4, 1 },
                    { 5, 1 },
                    { 3, 2 },
                    { 4, 2 },
                    { 5, 2 }
                });

            migrationBuilder.InsertData(
                table: "userContexts",
                columns: new[] { "Id", "CondoId", "OrganizationId", "Status", "UserId" },
                values: new object[] { new Guid("f1f1f1f1-f1f1-f1f1-f1f1-f1f1f1f1f1f1"), new Guid("c2c2c2c2-c2c2-c2c2-c2c2-c2c2c2c2c2c2"), new Guid("a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1"), 1, new Guid("d3d3d3d3-d3d3-d3d3-d3d3-d3d3d3d3d3d3") });

            migrationBuilder.CreateIndex(
                name: "IX_passwordResetTokens_UserId",
                table: "passwordResetTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_refreshTokens_UserId",
                table: "refreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_rolePermissions_PermissionId",
                table: "rolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_userContextRoles_RolesId",
                table: "userContextRoles",
                column: "RolesId");

            migrationBuilder.CreateIndex(
                name: "IX_userContexts_OrganizationId_CondoId",
                table: "userContexts",
                columns: new[] { "OrganizationId", "CondoId" });

            migrationBuilder.CreateIndex(
                name: "IX_userContexts_UserId",
                table: "userContexts",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "apiKeys");

            migrationBuilder.DropTable(
                name: "passwordResetTokens");

            migrationBuilder.DropTable(
                name: "refreshTokens");

            migrationBuilder.DropTable(
                name: "rolePermissions");

            migrationBuilder.DropTable(
                name: "userContextRoles");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "userContexts");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
