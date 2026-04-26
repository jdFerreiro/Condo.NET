using CondoNet.Shared.DTOs.Financial;

namespace CondoNet.Financial.Core.Interfaces
{
    public interface IFinancialService
    {
        Task RegisterPaymentAsync(RegisterPaymentDto payment, CancellationToken cancellationToken = default);
        Task<AccountStatusDto> GetAccountStatusAsync(string unitId, CancellationToken cancellationToken = default);
        Task SplitFundsAsync(RegisterPaymentDto payment, CancellationToken cancellationToken = default);
    }
}
