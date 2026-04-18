using AgendeX.Application.Common.Interfaces;
using AgendeX.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AgendeX.Infrastructure.Auth;

public sealed class TokenService : ITokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly SigningCredentials _signingCredentials;

    public TokenService(IOptions<JwtOptions> jwtOptions, RsaKeyProvider rsaKeyProvider)
    {
        _jwtOptions = jwtOptions.Value;
        ValidateOptions(_jwtOptions);
        _signingCredentials = new SigningCredentials(rsaKeyProvider.PrivateKey, SecurityAlgorithms.RsaSha256);
    }

    public string GenerateAccessToken(User user)
    {
        DateTime expiresAt = GetAccessTokenExpiryUtc();

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Name, user.Name),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];

        JwtSecurityToken jwtToken = new(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: _signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }

    public string GenerateRefreshToken()
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    public string ComputeSha256Hash(string value)
    {
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public DateTime GetAccessTokenExpiryUtc() =>
        DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);

    public DateTime GetRefreshTokenExpiryUtc() =>
        DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);

    private static void ValidateOptions(JwtOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Issuer) || string.IsNullOrWhiteSpace(options.Audience))
        {
            throw new InvalidOperationException("JWT issuer and audience must be configured.");
        }
    }
}
