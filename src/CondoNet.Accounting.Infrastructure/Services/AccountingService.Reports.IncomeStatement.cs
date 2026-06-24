using CondoNet.Accounting.Core.Entities;
using CondoNet.Shared;
using CondoNet.Shared.Accounting.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Accounting.Infrastructure.Services
{
    public partial class AccountingService
    {
        // 1. GENERAR ESTADO DE RESULTADOS (Ingresos - Gastos = Utilidad/Pérdida)
        public async Task<Result<IncomeStatementResponse>> GetIncomeStatementAsync(DateTime from, DateTime to)
        {
            var condominiumId = tenantService.GetCondominiumId();

            // Extraer todos los renglones de diario en el rango de fechas que correspondan a comprobantes válidos (Posted)
            var entries = await context.Set<AccountingEntry>()
                .Include(e => e.Account)
                .Where(e => e.CondominiumId == condominiumId
                            && e.Transaction.Date >= from
                            && e.Transaction.Date <= to
                            && e.Transaction.Status == TransactionStatus.Posted)
                .ToListAsync();

            // Agrupar los movimientos por cuenta contable para consolidar los saldos del periodo
            var accountMovements = entries
                .GroupBy(e => e.Account)
                .Select(g => new
                {
                    Account = g.Key,
                    PeriodBalance = g.Sum(e => CalculateBalanceImpactLocal(g.Key.Type, e.Debit, e.Credit))
                })
                .ToList();

            // Filtrar y clasificar en Ingresos (Revenues) y Egresos/Gastos (Expenses)
            var revenues = accountMovements
                .Where(am => am.Account.Type == Account.AccountType.Revenue)
                .Select(am => new BalanceItemDto(am.Account.Code, am.Account.Name, am.PeriodBalance))
                .ToList();

            var expenses = accountMovements
                .Where(am => am.Account.Type == Account.AccountType.Expense)
                .Select(am => new BalanceItemDto(am.Account.Code, am.Account.Name, am.PeriodBalance))
                .ToList();

            // Totales y Utilidad Neta del Ejercicio
            decimal totalRevenues = revenues.Sum(r => r.Balance);
            decimal totalExpenses = expenses.Sum(e => e.Balance);
            decimal netIncome = totalRevenues - totalExpenses;

            var report = new IncomeStatementResponse(
                Revenues: revenues,
                Expenses: expenses,
                TotalRevenues: totalRevenues,
                TotalExpenses: totalExpenses,
                NetIncome: netIncome
            );

            return Result<IncomeStatementResponse>.Success(report);
        }
    }
}
