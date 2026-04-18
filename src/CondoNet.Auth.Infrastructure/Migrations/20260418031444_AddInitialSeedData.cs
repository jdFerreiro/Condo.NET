using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CondoNet.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInitialSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash" },
                values: new object[] { new Guid("d3d3d3d3-d3d3-d3d3-d3d3-d3d3d3d3d3d3"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@condo.net", "Admin Maestro", true, "$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgNI9bPaoBRW61dk8K.6zZ6qO6kO" });

            migrationBuilder.InsertData(
                table: "user_contexts",
                columns: new[] { "Id", "CondoId", "OrganizationId", "Role", "UserId" },
                values: new object[] { new Guid("f1f1f1f1-f1f1-f1f1-f1f1-f1f1f1f1f1f1"), new Guid("c2c2c2c2-c2c2-c2c2-c2c2-c2c2c2c2c2c2"), new Guid("a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1"), "ADMIN", new Guid("d3d3d3d3-d3d3-d3d3-d3d3-d3d3d3d3d3d3") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "user_contexts",
                keyColumn: "Id",
                keyValue: new Guid("f1f1f1f1-f1f1-f1f1-f1f1-f1f1f1f1f1f1"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("d3d3d3d3-d3d3-d3d3-d3d3-d3d3d3d3d3d3"));
        }
    }
}
