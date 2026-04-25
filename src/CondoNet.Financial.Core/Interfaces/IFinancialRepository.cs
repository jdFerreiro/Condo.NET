
using CondoNet.Financial.Core.Entities;

namespace CondoNet.Financial.Core.Interfaces
{
    public interface IFinancialRepository
    {
        Task<decimal> GetBalanceAsync(int accountId);
        Task AddTransactionAsync(Transaction entity);
        // Otros métodos de acceso a datos...
    }
}
