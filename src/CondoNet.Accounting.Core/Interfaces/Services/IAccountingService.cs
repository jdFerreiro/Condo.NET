using CondoNet.Shared;
using CondoNet.Shared.Accounting.DTOs;

namespace CondoNet.Accounting.Core.Interfaces.Services
{
    public interface IAccountingService
    {
        Task<Result<Guid>> CreateAccountAsync(CreateAccountRequest request);
        Task<Result<bool>> ToggleAccountStatusAsync(Guid accountId, bool isActive);
    }
}