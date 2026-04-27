using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CondoNet.Financial.Core.Entities;

namespace CondoNet.Financial.Core.Interfaces;

public interface IBankTransactionRepository
{
    Task<IEnumerable<BankTransaction>> GetUnmatchedAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(BankTransaction transaction, CancellationToken cancellationToken = default);
}
