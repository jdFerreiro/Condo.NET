using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CondoNet.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApiKeyTable : Migration
    {
        private static readonly string[] columns = ["Id", "CreatedAt", "Description", "IsActive", "Key", "OrganizationId"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "apiKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apiKeys", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "apiKeys",
                columns: columns,
                values: [new Guid("e4e4e4e4-e4e4-e4e4-e4e4-e4e4e4e4e4e4"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Llave de desarrollo para Postman", true, "$2a$11$qR7iHhZ9eP0vU7E.8G9m6.fP0D7R8r9m6.fP0D7R8r9m6.fP0D7R", new Guid("a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1")]);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("d3d3d3d3-d3d3-d3d3-d3d3-d3d3d3d3d3d3"),
                column: "PasswordHash",
                value: "$2a$11$UJFPSGUU2mYbVcEDyWSPqOLRX8CM8qoWnMhKBGlX6y2xAdGy6YuLi");

            migrationBuilder.CreateIndex(
                name: "IX_apiKeys_Key",
                table: "apiKeys",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "apiKeys");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("d3d3d3d3-d3d3-d3d3-d3d3-d3d3d3d3d3d3"),
                column: "PasswordHash",
                value: "$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgNI9bPaoBRW61dk8K.6zZ6qO6kO");
        }
    }
}
