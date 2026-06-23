using CondoNet.Accounting.Core.Entities;

namespace CondoNet.Accounting.Infrastructure.Services
{
    public partial class AccountingService
    {
        /// <summary>
        /// Calcula el impacto neto en el saldo según la naturaleza contable de la cuenta.
        /// Cuentas Deudoras (Activos, Gastos) suman Débitos, restan Créditos.
        /// Cuentas Acreedoras (Pasivos, Patrimonio, Ingresos) suman Créditos, restan Débitos.
        /// </summary>
        public static decimal CalculateBalanceImpact(Account.AccountType type, decimal debit, decimal credit)
        {
            return type switch
            {
                Account.AccountType.Asset or Account.AccountType.Expense => debit - credit,
                Account.AccountType.Liability or Account.AccountType.Equity or Account.AccountType.Revenue => credit - debit,
                _ => 0
            };
        }

    }
}
