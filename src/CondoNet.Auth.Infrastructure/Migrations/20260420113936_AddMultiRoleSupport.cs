using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CondoNet.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiRoleSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_userContexts_roles_RoleId",
                table: "userContexts");

            migrationBuilder.DropIndex(
                name: "IX_userContexts_RoleId",
                table: "userContexts");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "userContexts");

            migrationBuilder.CreateTable(
                name: "userContextRoles",
                columns: table => new
                {
                    ContextsId = table.Column<Guid>(type: "uuid", nullable: false),
                    RolesId = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_userContextRoles_RolesId",
                table: "userContextRoles",
                column: "RolesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "userContextRoles");

            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "userContexts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "userContexts",
                keyColumn: "Id",
                keyValue: new Guid("f1f1f1f1-f1f1-f1f1-f1f1-f1f1f1f1f1f1"),
                column: "RoleId",
                value: 1);

            migrationBuilder.CreateIndex(
                name: "IX_userContexts_RoleId",
                table: "userContexts",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_userContexts_roles_RoleId",
                table: "userContexts",
                column: "RoleId",
                principalTable: "roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
