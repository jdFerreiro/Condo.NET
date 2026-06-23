using CondoNet.Accounting.Core.Entities;
using System.Threading.Tasks;

namespace CondoNet.Accounting.Core.Interfaces.Services;

public interface IAccountingAutomatonService
{
    /// <summary>
    /// Procesa una transacción contable, valida reglas y actualiza cuentas.
    /// </summary>
    Task ProcessTransactionAsync(AccountingTransaction transaction, Guid userId);
}
