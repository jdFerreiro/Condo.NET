using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CondoNet.Financial.Core.Entities;

namespace CondoNet.Financial.Core.Interfaces;

public interface IReportedPaymentRepository
{
    Task<IEnumerable<ReportedPayment>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(ReportedPayment payment, CancellationToken cancellationToken = default);
}
