using CondoNet.Accounting.Core.Entities;
using CondoNet.Shared;
using CondoNet.Shared.Accounting.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Accounting.Infrastructure.Services
{
    public partial class AccountingService
    {
        // 1. GENERAR BALANCE GENERAL (Activo = Pasivo + Patrimonio)
        public async Task<Result<BalanceSheetResponse>> GetBalanceSheetAsync()
        {
            var condominiumId = tenantService.GetCondominiumId();

            // Consultar únicamente las cuentas activas asignadas a este condominio
            var accounts = await context.Set<Account>()
                .Where(a => a.CondominiumId == condominiumId && a.IsActive)
                .ToListAsync();

            // Filtrar y mapear quirúrgicamente por tipo de cuenta utilizando el DTO reutilizable
            var assets = accounts
                .Where(a => a.Type == Account.AccountType.Asset)
                .Select(MapToBalanceItem)
                .ToList();

            var liabilities = accounts
                .Where(a => a.Type == Account.AccountType.Liability)
                .Select(MapToBalanceItem)
                .ToList();

            var equity = accounts
                .Where(a => a.Type == Account.AccountType.Equity)
                .Select(MapToBalanceItem)
                .ToList();

            // Cálculos matemáticos de validación de la ecuación contable
            decimal totalAssets = assets.Sum(a => a.Balance);
            decimal totalLiabilitiesAndEquity = liabilities.Sum(l => l.Balance) + equity.Sum(e => e.Balance);

            var report = new BalanceSheetResponse(
                Assets: assets,
                Liabilities: liabilities,
                Equity: equity,
                TotalAssets: totalAssets,
                TotalLiabilitiesAndEquity: totalLiabilitiesAndEquity
            );

            return Result<BalanceSheetResponse>.Success(report);
        }

        #region Mapeador Privado de Reportes

        private static BalanceItemDto MapToBalanceItem(Account account)
        {
            return new BalanceItemDto(
                AccountCode: account.Code,
                AccountName: account.Name,
                Balance: account.CurrentBalance
            );
        }

        #endregion
    }
}

