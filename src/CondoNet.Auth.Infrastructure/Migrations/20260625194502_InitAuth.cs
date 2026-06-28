using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CondoNet.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitAuth : Migration
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
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ParentPermissionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_permissions_permissions_ParentPermissionId",
                        column: x => x.ParentPermissionId,
                        principalTable: "permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                columns: new[] { "Id", "Description", "DisplayOrder", "Icon", "Name", "ParentPermissionId", "Path" },
                values: new object[,]
                {
                    { 10000, "Métricas consolidadas de recaudación y ocupación", 1, "layout-dashboard", "Dashboard Global", null, "/dashboard" },
                    { 20000, "Configuración física e inquilinos del ERP", 2, "building", "Estructura y Activos", null, "#" },
                    { 30000, "Gestión del padrón de copropietarios y residentes", 3, "users", "Comunidad", null, "#" },
                    { 40000, "Explotación comercial de áreas comunes y estacionamientos", 4, "calendar-days", "Servicios y Áreas", null, "#" },
                    { 50000, "Motor de cuentas corrientes y emisión de recibos", 5, "credit-card", "Facturación y Cobros", null, "#" },
                    { 60000, "Libro Mayor, Partida Doble y Autómatas Contables", 6, "landmark", "Contabilidad Core", null, "#" },
                    { 90000, "Seguridad informática, accesos y auditoría forense", 99, "sliders", "Configuración Sistema", null, "#" }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "SUPERADMIN" },
                    { 2, "ADMIN" },
                    { 3, "PROPIETARIO" },
                    { 4, "INQUILINO" },
                    { 5, "EXTERNO" },
                    { 6, "CONSERJE" },
                    { 7, "JUNTA DE CONDOMINIO" },
                    { 8, "PROVEEDOR" },
                    { 9, "ADMINISTRADOR DE PROPIEDAD" },
                    { 10, "ADMINISTRADOR DE ESTACIONAMIENTO" },
                    { 11, "AUDITO" }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash" },
                values: new object[] { new Guid("d3d3d3d3-d3d3-d3d3-d3d3-d3d3d3d3d3d3"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@condo.net", "Admin Maestro", true, "$2a$11$UJFPSGUU2mYbVcEDyWSPqOLRX8CM8qoWnMhKBGlX6y2xAdGy6YuLi" });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "Id", "Description", "DisplayOrder", "Icon", "Name", "ParentPermissionId", "Path" },
                values: new object[,]
                {
                    { 20100, "Gestión de empresas administradoras multi-tenant", 1, "briefcase", "Organizaciones (Admin)", 20000, "/assets/organizations" },
                    { 20200, "Onboarding y gestión de edificios/complejos", 2, "home", "Condominios", 20000, "/assets/condominiums" },
                    { 20300, "Subdivisiones físicas de la infraestructura", 3, "layers", "Torres y Bloques", 20000, "/assets/towers" },
                    { 20400, "Mantenimiento, áreas y alícuotas de apartamentos/locales", 4, "door-closed", "Unidades", 20000, "/assets/units" },
                    { 20500, "Importador diferido asíncrono de infraestructura", 5, "file-spreadsheet", "Carga Masiva (Bulk)", 20000, "/assets/bulk-import" },
                    { 30100, "Directorio legal de dueños de inmuebles", 1, "user-check", "Propietarios", 30000, "/community/owners" },
                    { 30200, "Control de habitantes, cargas familiares y mascotas", 2, "smile", "Inquilinos y Residentes", 30000, "/community/residents" },
                    { 30300, "Control de tags y patentes vehiculares permitidas", 3, "car", "Vehículos y Accesos", 30000, "/community/vehicles" },
                    { 40100, "Alquiler y control de puestos de parking a terceros", 1, "truck", "Estacionamientos Externos", 40000, "/services/parking" },
                    { 40200, "Alquiler controlado de salones de fiesta, parrilleras, etc.", 2, "palmtree", "Reserva de Amenities", 40000, "/services/amenities" },
                    { 50100, "Registro de planillas de gastos del mes (Luz, Agua, etc.)", 1, "receipt", "Gastos Comunes", 50000, "/billing/expenses" },
                    { 50200, "Procesamiento masivo de cuotas de mantenimiento", 2, "file-invoice-dollar", "Emisión de Recibos", 50000, "/billing/invoice-generation" },
                    { 50300, "Consulta de saldos de las unidades habitacionales", 3, "wallet", "Estados de Cuenta", 50000, "/billing/statements" },
                    { 50400, "Aprobación de transferencias reportadas por propietarios", 4, "check-square", "Validación de Pagos", 50000, "/billing/payments-validation" },
                    { 60100, "CRUD visual y jerárquico del árbol contable", 1, "git-fork", "Plan de Cuentas", 60000, "/accounting/chart-of-accounts" },
                    { 60200, "Registro manual, consulta y anulación quirúrgica de asientos", 2, "book", "Libro Diario", 60000, "/accounting/journal-entries" },
                    { 60300, "Configuración de plantillas e impuestos basados en eventos", 3, "cpu", "Autómata Contable", 60000, "/accounting/automaton-templates" },
                    { 60400, "Reporte automatizado de Activos, Pasivos y Patrimonio", 4, "scale", "Balance General", 60000, "/accounting/reports/balance-sheet" },
                    { 60500, "Estado dinámico de Ganancias y Pérdidas del ejercicio", 5, "trending-up", "Estado de Resultados", 60000, "/accounting/reports/income-statement" },
                    { 90100, "Control de credenciales, perfiles y bloqueos", 1, "user-cog", "Gestión de Usuarios", 90000, "/system/users" },
                    { 90200, "Definición de perfiles (Admin, Contador, Comité)", 2, "shield-check", "Roles del Sistema", 90000, "/system/roles" },
                    { 90300, "Mapeo dinámico de accesos por rol", 3, "lock", "Matriz de Permisos", 90000, "/system/permissions" },
                    { 90400, "Tokens de integración para pasarelas o hardware externo", 4, "key-round", "Claves de API (ApiKeys)", 90000, "/system/apikeys" },
                    { 90500, "Trazabilidad inmutable de acciones críticas de usuarios", 5, "file-search", "Auditoría Forense", 90000, "/system/audit-logs" }
                });

            migrationBuilder.InsertData(
                table: "rolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 10000, 1 },
                    { 20000, 1 },
                    { 30000, 1 },
                    { 40000, 1 },
                    { 50000, 1 },
                    { 60000, 1 },
                    { 90000, 1 }
                });

            migrationBuilder.InsertData(
                table: "userContexts",
                columns: new[] { "Id", "CondoId", "OrganizationId", "Status", "UserId" },
                values: new object[] { new Guid("f1f1f1f1-f1f1-f1f1-f1f1-f1f1f1f1f1f1"), new Guid("c2c2c2c2-c2c2-c2c2-c2c2-c2c2c2c2c2c2"), new Guid("a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1"), 1, new Guid("d3d3d3d3-d3d3-d3d3-d3d3-d3d3d3d3d3d3") });

            migrationBuilder.InsertData(
                table: "rolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 20100, 1 },
                    { 20200, 1 },
                    { 20300, 1 },
                    { 20400, 1 },
                    { 20500, 1 },
                    { 30100, 1 },
                    { 30200, 1 },
                    { 30300, 1 },
                    { 40100, 1 },
                    { 40200, 1 },
                    { 50100, 1 },
                    { 50200, 1 },
                    { 50300, 1 },
                    { 50400, 1 },
                    { 60100, 1 },
                    { 60200, 1 },
                    { 60300, 1 },
                    { 60400, 1 },
                    { 60500, 1 },
                    { 90100, 1 },
                    { 90200, 1 },
                    { 90300, 1 },
                    { 90400, 1 },
                    { 90500, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_passwordResetTokens_UserId",
                table: "passwordResetTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_permissions_ParentPermissionId",
                table: "permissions",
                column: "ParentPermissionId");

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
