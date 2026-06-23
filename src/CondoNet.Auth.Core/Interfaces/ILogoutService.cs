using CondoNet.Shared;

namespace CondoNet.Auth.Core.Interfaces
{
    public interface ILogoutService
    {
        Task<Result<bool>> LogoutAsync(string refreshToken);
    }
}
