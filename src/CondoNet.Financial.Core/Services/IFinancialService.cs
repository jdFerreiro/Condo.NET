using CondoNet.Shared.Financial.DTOs;
using CondoNet.Shared.Payment.DTOs;

namespace CondoNet.Financial.Core.Services
{
    public interface IFinancialService
    {
        Task RegisterPaymentAsync(RegisterPaymentDto payment, CancellationToken cancellationToken = default);
        Task<AccountStatusDto> GetAccountStatusAsync(string unitId, CancellationToken cancellationToken = default);
        Task SplitFundsAsync(RegisterPaymentDto payment, CancellationToken cancellationToken = default);
        Task CertifyPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default);
    }
}
