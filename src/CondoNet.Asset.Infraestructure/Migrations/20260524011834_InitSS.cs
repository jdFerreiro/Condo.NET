using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CondoNet.Asset.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitSS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "criticalequipments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CondominiumId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InstallationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaintenanceFrequencyDays = table.Column<int>(type: "int", nullable: false),
                    ManualUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_criticalequipments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TaxId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BaseCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Plan = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "condominiums",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TaxId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReserveFundPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_condominiums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_condominiums_organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "towers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CondominiumId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_towers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_towers_condominiums_CondominiumId",
                        column: x => x.CondominiumId,
                        principalTable: "condominiums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "units",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TowerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Identifier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Floor = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AreaSquareMeters = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Alias = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aliquot = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    OwnerEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CondominiumId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_units", x => x.Id);
                    table.ForeignKey(
                        name: "FK_units_condominiums_CondominiumId",
                        column: x => x.CondominiumId,
                        principalTable: "condominiums",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_units_towers_TowerId",
                        column: x => x.TowerId,
                        principalTable: "towers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CondominiumId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    IsRentable = table.Column<bool>(type: "bit", nullable: false),
                    DefaultRentalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LinkedUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_assets_condominiums_CondominiumId",
                        column: x => x.CondominiumId,
                        principalTable: "condominiums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_assets_units_LinkedUnitId",
                        column: x => x.LinkedUnitId,
                        principalTable: "units",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "organizations",
                columns: new[] { "Id", "BaseCurrency", "ContactEmail", "CreatedAt", "IsActive", "LogoUrl", "Name", "PhoneNumber", "Plan", "TaxId" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), "USD", "admin@condonet.test", new DateTime(2026, 4, 22, 11, 41, 25, 511, DateTimeKind.Utc).AddTicks(4964), true, null, "Administradora Global CondoNet", null, 0, "J-12345678-0" });

            migrationBuilder.InsertData(
                table: "condominiums",
                columns: new[] { "Id", "Address", "Name", "OrganizationId", "ReserveFundPercentage", "TaxId" },
                values: new object[] { new Guid("22222222-2222-2222-2222-222222222222"), "Av. Principal, Edificio Sol y Mar", "Residencias Sol y Mar", new Guid("11111111-1111-1111-1111-111111111111"), 10.00m, "J-87654321-0" });

            migrationBuilder.InsertData(
                table: "towers",
                columns: new[] { "Id", "CondominiumId", "Name", "OrganizationId" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333333"), new Guid("22222222-2222-2222-2222-222222222222"), "Torre A", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("44444444-4444-4444-4444-444444444444"), new Guid("22222222-2222-2222-2222-222222222222"), "Torre B", new Guid("11111111-1111-1111-1111-111111111111") }
                });

            migrationBuilder.InsertData(
                table: "units",
                columns: new[] { "Id", "Alias", "Aliquot", "AreaSquareMeters", "CondominiumId", "Floor", "Identifier", "OrganizationId", "OwnerEmail", "TowerId", "Type" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), null, 50.0000m, null, null, "1", "A01", new Guid("11111111-1111-1111-1111-111111111111"), "vecino101@test.com", new Guid("33333333-3333-3333-3333-333333333333"), 1 },
                    { new Guid("22222222-2222-2222-2222-222222222222"), null, 50.0000m, null, null, "1", "A02", new Guid("11111111-1111-1111-1111-111111111111"), "vecino102@test.com", new Guid("33333333-3333-3333-3333-333333333333"), 1 },
                    { new Guid("33333333-3333-3333-3333-333333333333"), null, 50.0000m, null, null, "1", "B01", new Guid("11111111-1111-1111-1111-111111111111"), "vecino101@test.com", new Guid("33333333-3333-3333-3333-333333333333"), 1 },
                    { new Guid("44444444-4444-4444-4444-444444444444"), null, 50.0000m, null, null, "1", "B02", new Guid("11111111-1111-1111-1111-111111111111"), "vecino102@test.com", new Guid("33333333-3333-3333-3333-333333333333"), 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_assets_CondominiumId",
                table: "assets",
                column: "CondominiumId");

            migrationBuilder.CreateIndex(
                name: "IX_assets_LinkedUnitId",
                table: "assets",
                column: "LinkedUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_condominiums_OrganizationId",
                table: "condominiums",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_TaxId",
                table: "organizations",
                column: "TaxId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_towers_CondominiumId",
                table: "towers",
                column: "CondominiumId");

            migrationBuilder.CreateIndex(
                name: "IX_units_CondominiumId",
                table: "units",
                column: "CondominiumId");

            migrationBuilder.CreateIndex(
                name: "IX_units_TowerId",
                table: "units",
                column: "TowerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assets");

            migrationBuilder.DropTable(
                name: "criticalequipments");

            migrationBuilder.DropTable(
                name: "units");

            migrationBuilder.DropTable(
                name: "towers");

            migrationBuilder.DropTable(
                name: "condominiums");

            migrationBuilder.DropTable(
                name: "organizations");
        }
    }
}
