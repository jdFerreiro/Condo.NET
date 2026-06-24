using CondoNet.Accounting.Core.Entities;
using CondoNet.Accounting.Core.Interfaces.Services;
using CondoNet.Shared;
using CondoNet.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Accounting.Infrastructure.Services
{
    public class AccountingSeeder(DbContext context, ITenantService tenantService) : IAccountingSeeder
    {
        // 1. DEFINICIÓN MANUAL SECUENCIAL DEL PLAN DE CUENTAS BÁSICO PARA UN CONDOMINIO
        private static readonly List<SeedAccountDefinition> BaseCatalogDefinitions =
        [
            // =========================================================================
            // 1. ACTIVOS
            // =========================================================================
            new("1", "ACTIVO", Account.AccountType.Asset, false, null),
            new("1.1", "ACTIVO CORRIENTE", Account.AccountType.Asset, false, "1"),
            
            // Disponibilidades (Efectivo y Bancos)
            new("1.1.01", "CAJA Y BANCOS", Account.AccountType.Asset, false, "1.1"),
            new("1.1.01.001", "Caja Chica Administración", Account.AccountType.Asset, true, "1.1.01"),
            new("1.1.01.002", "Banco Cuenta Principal", Account.AccountType.Asset, true, "1.1.01"),
            new("1.1.01.003", "Banco Cuenta Fondo de Reserva", Account.AccountType.Asset, true, "1.1.01"),
            
            // Cuentas por Cobrar (Clave para la emisión de recibos)
            new("1.1.02", "CUENTAS POR COBRAR", Account.AccountType.Asset, false, "1.1"),
            new("1.1.02.001", "Cuotas de Mantenimiento por Cobrar", Account.AccountType.Asset, true, "1.1.02"),
            new("1.1.02.002", "Cuotas Extraordinarias por Cobrar", Account.AccountType.Asset, true, "1.1.02"),
            new("1.1.02.003", "Intereses de Mora por Cobrar", Account.AccountType.Asset, true, "1.1.02"),

            // =========================================================================
            // 2. PASIVOS
            // =========================================================================
            new("2", "PASIVO", Account.AccountType.Liability, false, null),
            new("2.1", "PASIVO CORRIENTE", Account.AccountType.Liability, false, "2"),
            
            // Obligaciones con Proveedores (Gastos acumulados por pagar)
            new("2.1.01", "CUENTAS POR PAGAR", Account.AccountType.Liability, false, "2.1"),
            new("2.1.01.001", "Proveedores de Servicios Públicos por Pagar", Account.AccountType.Liability, true, "2.1.01"),
            new("2.1.01.002", "Contratistas y Mantenimiento por Pagar", Account.AccountType.Liability, true, "2.1.01"),
            
            // Cobros Anticipados
            new("2.1.02", "OBLIGACIONES CON PROPIETARIOS", Account.AccountType.Liability, false, "2.1"),
            new("2.1.02.001", "Pagos Anticipados de Propietarios", Account.AccountType.Liability, true, "2.1.02"),

            // =========================================================================
            // 3. PATRIMONIO
            // =========================================================================
            new("3", "PATRIMONIO", Account.AccountType.Equity, false, null),
            new("3.1", "CAPITAL Y FONDOS", Account.AccountType.Equity, false, "3"),
            new("3.1.01", "Fondos de Reserva Colectivos", Account.AccountType.Equity, true, "3.1"),
            new("3.1.02", "Fondo de Prestaciones Sociales Conserjería", Account.AccountType.Equity, true, "3.1"),
            new("3.1.03", "Utilidad o Pérdida de Ejercicios Anteriores", Account.AccountType.Equity, true, "3.1"),

            // =========================================================================
            // 4. INGRESOS
            // =========================================================================
            new("4", "INGRESOS", Account.AccountType.Revenue, false, null),
            new("4.1", "INGRESOS OPERACIONALES", Account.AccountType.Revenue, false, "4"),
            new("4.1.01", "Recaudación por Cuotas Ordinarias", Account.AccountType.Revenue, true, "4.1"),
            new("4.1.02", "Recaudación por Cuotas Extraordinarias", Account.AccountType.Revenue, true, "4.1"),
            new("4.1.03", "Ingresos por Intereses de Mora", Account.AccountType.Revenue, true, "4.1"),
            new("4.1.04", "Ingresos por Alquiler de Áreas Comunes (Salón de Fiesta/Parrillera)", Account.AccountType.Revenue, true, "4.1"),

            // =========================================================================
            // 5. EGRESOS / GASTOS
            // =========================================================================
            new("5", "EGRESOS", Account.AccountType.Expense, false, null),
            new("5.1", "GASTOS OPERACIONALES", Account.AccountType.Expense, false, "5"),
            
            // Gastos de Servicios Públicos
            new("5.1.01", "SERVICIOS PÚBLICOS", Account.AccountType.Expense, false, "5.1"),
            new("5.1.01.001", "Gasto de Energía Eléctrica (Áreas Comunes)", Account.AccountType.Expense, true, "5.1.01"),
            new("5.1.01.002", "Gasto de Agua Potable / Cisternas", Account.AccountType.Expense, true, "5.1.01"),
            new("5.1.01.003", "Gasto de Aseo Urbano y Limpieza", Account.AccountType.Expense, true, "5.1.01"),
            
            // Gastos de Operación y Personal
            new("5.1.02", "GASTOS DE MANTENIMIENTO Y REPARACIONES", Account.AccountType.Expense, false, "5.1"),
            new("5.1.02.001", "Mantenimiento de Ascensores", Account.AccountType.Expense, true, "5.1.02"),
            new("5.1.02.002", "Mantenimiento de Bombas de Agua y Piscinas", Account.AccountType.Expense, true, "5.1.02"),
            new("5.1.02.003", "Sueldos y Salarios Conserjería / Vigilancia", Account.AccountType.Expense, true, "5.1.02"),
            
            // Gastos Administrativos
            new("5.1.03", "GASTOS ADMINISTRATIVOS Y BANCARIOS", Account.AccountType.Expense, false, "5.1"),
            new("5.1.03.001", "Honorarios de Administración", Account.AccountType.Expense, true, "5.1.03"),
            new("5.1.03.002", "Comisiones Bancarias", Account.AccountType.Expense, true, "5.1.03")
        ];

        // 2. EJECUCIÓN DEL SEEDING CON RESOLUCIÓN DINÁMICA DE PADRES
        public async Task<Result<bool>> SeedBaseCatalogAsync(Guid? tenantCondoId = null, Guid? tenantOrgId = null)
        {
            // Si se pasan por parámetros (ej: Arranque inicial), los usa. Si no, los extrae del TenantService.
            var organizationId = tenantOrgId ?? tenantService.GetOrganizationId();
            var condominiumId = tenantCondoId ?? tenantService.GetCondominiumId();

            var alreadyHasAccounts = await context.Set<Account>()
                .AnyAsync(a => a.CondominiumId == condominiumId);

            if (alreadyHasAccounts)
                return Result<bool>.Failure("El condominio ya posee un catálogo de cuentas configurado.");

            // 3. Declaración de estructuras locales en memoria para resolver llaves foráneas
            var codeToIdMap = new Dictionary<string, Guid>();
            var accountsToInsert = new List<Account>(); // Aquí se declara formalmente la variable que faltaba

            // 4. Procesar secuencialmente la matriz de definiciones básicas
            foreach (var def in BaseCatalogDefinitions)
            {
                var accountId = Guid.NewGuid();
                codeToIdMap[def.Code] = accountId; // Registrar el ID asignado a este código

                Guid? parentId = null;
                if (!string.IsNullOrEmpty(def.ParentCode))
                {
                    // Vincular de forma automática el Guid del padre usando el mapa en memoria
                    if (codeToIdMap.TryGetValue(def.ParentCode, out var mappedParentId))
                    {
                        parentId = mappedParentId;
                    }
                }

                // Instanciar la entidad de dominio contable con aislamiento multi-tenant
                var account = new Account
                {
                    Id = accountId,
                    OrganizationId = organizationId,
                    CondominiumId = condominiumId,
                    Code = def.Code,
                    Name = def.Name,
                    Type = def.Type,
                    IsTransactional = def.IsTransactional,
                    ParentAccountId = parentId,
                    CurrentBalance = 0,
                    IsActive = true
                };

                accountsToInsert.Add(account);
            }

            await context.Set<Account>().AddRangeAsync(accountsToInsert);
            await context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }

    // Sub-DTO interno e inmutable exclusivo para estructurar la matriz del seed
    internal record SeedAccountDefinition(
        string Code,
        string Name,
        Account.AccountType Type,
        bool IsTransactional,
        string? ParentCode
    );
}
