using CondoNet.Accounting.Core.Entities;

namespace CondoNet.Accounting.Infrastructure.Services
{
    public partial class AccountingService
    {
        // Debe ser private o internal static, nunca parte de la interfaz pública
        public decimal CalculateBalanceImpactLocal(Account.AccountType type, decimal debit, decimal credit)
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
