using AgendeX.Domain.Entities;

namespace AgendeX.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    string ComputeSha256Hash(string value);
    DateTime GetAccessTokenExpiryUtc();
    DateTime GetRefreshTokenExpiryUtc();
}
