using CondoNet.Auth.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CondoNet.Auth.Infrastructure.Configurations
{
    public class PermissionSeedConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.HasData(
                // =========================================================================
                // MÓDULO 10000: PANEL DE CONTROL Y MÉTRICAS (RAÍZ)
                // =========================================================================
                new Permission { Id = 10000, Name = "Dashboard Global", Path = "/dashboard", Description = "Métricas consolidadas de recaudación y ocupación", Icon = "layout-dashboard", DisplayOrder = 1, ParentPermissionId = null },

                // =========================================================================
                // MÓDULO 20000: ESTRUCTURA OPERATIVA Y ACTIVOS (PADRE)
                // =========================================================================
                new Permission { Id = 20000, Name = "Estructura y Activos", Path = "#", Description = "Configuración física e inquilinos del ERP", Icon = "building", DisplayOrder = 2, ParentPermissionId = null },

                new Permission { Id = 20100, Name = "Organizaciones (Admin)", Path = "/assets/organizations", Description = "Gestión de empresas administradoras multi-tenant", Icon = "briefcase", DisplayOrder = 1, ParentPermissionId = 20000 },
                new Permission { Id = 20200, Name = "Condominios", Path = "/assets/condominiums", Description = "Onboarding y gestión de edificios/complejos", Icon = "home", DisplayOrder = 2, ParentPermissionId = 20000 },
                new Permission { Id = 20300, Name = "Torres y Bloques", Path = "/assets/towers", Description = "Subdivisiones físicas de la infraestructura", Icon = "layers", DisplayOrder = 3, ParentPermissionId = 20000 },
                new Permission { Id = 20400, Name = "Unidades", Path = "/assets/units", Description = "Mantenimiento, áreas y alícuotas de apartamentos/locales", Icon = "door-closed", DisplayOrder = 4, ParentPermissionId = 20000 },
                new Permission { Id = 20500, Name = "Carga Masiva (Bulk)", Path = "/assets/bulk-import", Description = "Importador diferido asíncrono de infraestructura", Icon = "file-spreadsheet", DisplayOrder = 5, ParentPermissionId = 20000 },

                // =========================================================================
                // MÓDULO 30000: COMUNIDAD Y RESIDENTES (PADRE)
                // =========================================================================
                new Permission { Id = 30000, Name = "Comunidad", Path = "#", Description = "Gestión del padrón de copropietarios y residentes", Icon = "users", DisplayOrder = 3, ParentPermissionId = null },

                new Permission { Id = 30100, Name = "Propietarios", Path = "/community/owners", Description = "Directorio legal de dueños de inmuebles", Icon = "user-check", DisplayOrder = 1, ParentPermissionId = 30000 },
                new Permission { Id = 30200, Name = "Inquilinos y Residentes", Path = "/community/residents", Description = "Control de habitantes, cargas familiares y mascotas", Icon = "smile", DisplayOrder = 2, ParentPermissionId = 30000 },
                new Permission { Id = 30300, Name = "Vehículos y Accesos", Path = "/community/vehicles", Description = "Control de tags y patentes vehiculares permitidas", Icon = "car", DisplayOrder = 3, ParentPermissionId = 30000 },

                // =========================================================================
                // MÓDULO 40000: OPERACIONES EXTERNAS Y AMENITIES (PADRE)
                // =========================================================================
                new Permission { Id = 40000, Name = "Servicios y Áreas", Path = "#", Description = "Explotación comercial de áreas comunes y estacionamientos", Icon = "calendar-days", DisplayOrder = 4, ParentPermissionId = null },

                new Permission { Id = 40100, Name = "Estacionamientos Externos", Path = "/services/parking", Description = "Alquiler y control de puestos de parking a terceros", Icon = "truck", DisplayOrder = 1, ParentPermissionId = 40000 },
                new Permission { Id = 40200, Name = "Reserva de Amenities", Path = "/services/amenities", Description = "Alquiler controlado de salones de fiesta, parrilleras, etc.", Icon = "palmtree", DisplayOrder = 2, ParentPermissionId = 40000 },

                // =========================================================================
                // MÓDULO 50000: FACTURACIÓN Y RECAUDACIÓN / BILLING (PADRE)
                // =========================================================================
                new Permission { Id = 50000, Name = "Facturación y Cobros", Path = "#", Description = "Motor de cuentas corrientes y emisión de recibos", Icon = "credit-card", DisplayOrder = 5, ParentPermissionId = null },

                new Permission { Id = 50100, Name = "Gastos Comunes", Path = "/billing/expenses", Description = "Registro de planillas de gastos del mes (Luz, Agua, etc.)", Icon = "receipt", DisplayOrder = 1, ParentPermissionId = 50000 },
                new Permission { Id = 50200, Name = "Emisión de Recibos", Path = "/billing/invoice-generation", Description = "Procesamiento masivo de cuotas de mantenimiento", Icon = "file-invoice-dollar", DisplayOrder = 2, ParentPermissionId = 50000 },
                new Permission { Id = 50300, Name = "Estados de Cuenta", Path = "/billing/statements", Description = "Consulta de saldos de las unidades habitacionales", Icon = "wallet", DisplayOrder = 3, ParentPermissionId = 50000 },
                new Permission { Id = 50400, Name = "Validación de Pagos", Path = "/billing/payments-validation", Description = "Aprobación de transferencias reportadas por propietarios", Icon = "check-square", DisplayOrder = 4, ParentPermissionId = 50000 },

                // =========================================================================
                // MÓDULO 60000: CORE CONTABLE Y FINANZAS / ACCOUNTING (PADRE)
                // =========================================================================
                new Permission { Id = 60000, Name = "Contabilidad Core", Path = "#", Description = "Libro Mayor, Partida Doble y Autómatas Contables", Icon = "landmark", DisplayOrder = 6, ParentPermissionId = null },

                new Permission { Id = 60100, Name = "Plan de Cuentas", Path = "/accounting/chart-of-accounts", Description = "CRUD visual y jerárquico del árbol contable", Icon = "git-fork", DisplayOrder = 1, ParentPermissionId = 60000 },
                new Permission { Id = 60200, Name = "Libro Diario", Path = "/accounting/journal-entries", Description = "Registro manual, consulta y anulación quirúrgica de asientos", Icon = "book", DisplayOrder = 2, ParentPermissionId = 60000 },
                new Permission { Id = 60300, Name = "Autómata Contable", Path = "/accounting/automaton-templates", Description = "Configuración de plantillas e impuestos basados en eventos", Icon = "cpu", DisplayOrder = 3, ParentPermissionId = 60000 },
                new Permission { Id = 60400, Name = "Balance General", Path = "/accounting/reports/balance-sheet", Description = "Reporte automatizado de Activos, Pasivos y Patrimonio", Icon = "scale", DisplayOrder = 4, ParentPermissionId = 60000 },
                new Permission { Id = 60500, Name = "Estado de Resultados", Path = "/accounting/reports/income-statement", Description = "Estado dinámico de Ganancias y Pérdidas del ejercicio", Icon = "trending-up", DisplayOrder = 5, ParentPermissionId = 60000 },

                // =========================================================================
                // MÓDULO 90000: SEGURIDAD, API Y SISTEMA / AUTH (PADRE)
                // =========================================================================
                new Permission { Id = 90000, Name = "Configuración Sistema", Path = "#", Description = "Seguridad informática, accesos y auditoría forense", Icon = "sliders", DisplayOrder = 99, ParentPermissionId = null },

                new Permission { Id = 90100, Name = "Gestión de Usuarios", Path = "/system/users", Description = "Control de credenciales, perfiles y bloqueos", Icon = "user-cog", DisplayOrder = 1, ParentPermissionId = 90000 },
                new Permission { Id = 90200, Name = "Roles del Sistema", Path = "/system/roles", Description = "Definición de perfiles (Admin, Contador, Comité)", Icon = "shield-check", DisplayOrder = 2, ParentPermissionId = 90000 },
                new Permission { Id = 90300, Name = "Matriz de Permisos", Path = "/system/permissions", Description = "Mapeo dinámico de accesos por rol", Icon = "lock", DisplayOrder = 3, ParentPermissionId = 90000 },
                new Permission { Id = 90400, Name = "Claves de API (ApiKeys)", Path = "/system/apikeys", Description = "Tokens de integración para pasarelas o hardware externo", Icon = "key-round", DisplayOrder = 4, ParentPermissionId = 90000 },
                new Permission { Id = 90500, Name = "Auditoría Forense", Path = "/system/audit-logs", Description = "Trazabilidad inmutable de acciones críticas de usuarios", Icon = "file-search", DisplayOrder = 5, ParentPermissionId = 90000 }
            );
        }
    }
}
