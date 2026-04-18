using CondoNet.Auth.Core.Entities;

namespace CondoNet.Auth.Core.Interfaces;

public interface IIdentityService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
    string GenerateJwtToken(User user, UserContext context);
    string GenerateRefreshToken();
}
