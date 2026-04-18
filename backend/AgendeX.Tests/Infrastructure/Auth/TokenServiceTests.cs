using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Infrastructure.Identity;
using AgendeX.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AgendeX.Tests.Infrastructure.Auth;

public sealed class TokenServiceTests
{
    [Fact]
    public void GenerateAccessToken_ContainsExpectedClaimsAndIsValid()
    {
        JwtOptions jwtOptions = new()
        {
            Issuer = "AgendeX",
            Audience = "AgendeX.Clients",
            AccessTokenMinutes = 15,
            RefreshTokenDays = 7
        };

        using RsaKeyProvider rsaKeyProvider = new();
        TokenService tokenService = new(Options.Create(jwtOptions), rsaKeyProvider);

        User user = new("Bruna", "bruna@email.com", "hash", UserRole.Agent);

        string accessToken = tokenService.GenerateAccessToken(user);

        JwtSecurityTokenHandler tokenHandler = new();

        ClaimsPrincipal principal = tokenHandler.ValidateToken(accessToken, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = rsaKeyProvider.PublicKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        }, out SecurityToken validatedToken);

        validatedToken.Should().BeOfType<JwtSecurityToken>();
        principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value.Should().Be(user.Id.ToString());
        principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value.Should().Be(user.Email);
        principal.FindFirst(JwtRegisteredClaimNames.Name)?.Value.Should().Be(user.Name);
        principal.FindFirst(ClaimTypes.Role)?.Value.Should().Be(user.Role.ToString());
        principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsBase64ValueWithExpectedLength()
    {
        JwtOptions jwtOptions = new() { Issuer = "issuer", Audience = "audience" };

        using RsaKeyProvider rsaKeyProvider = new();
        TokenService tokenService = new(Options.Create(jwtOptions), rsaKeyProvider);

        string refreshToken = tokenService.GenerateRefreshToken();

        refreshToken.Should().NotBeNullOrWhiteSpace();
        refreshToken.Length.Should().Be(88);
    }

    [Fact]
    public void ComputeSha256Hash_ReturnsDeterministicHash()
    {
        JwtOptions jwtOptions = new() { Issuer = "issuer", Audience = "audience" };

        using RsaKeyProvider rsaKeyProvider = new();
        TokenService tokenService = new(Options.Create(jwtOptions), rsaKeyProvider);

        string hash = tokenService.ComputeSha256Hash("abc");

        hash.Should().Be("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad");
    }

    [Fact]
    public void ExpiryMethods_ReturnConfiguredIntervals()
    {
        JwtOptions jwtOptions = new()
        {
            Issuer = "issuer",
            Audience = "audience",
            AccessTokenMinutes = 20,
            RefreshTokenDays = 10
        };

        using RsaKeyProvider rsaKeyProvider = new();
        TokenService tokenService = new(Options.Create(jwtOptions), rsaKeyProvider);

        DateTime before = DateTime.UtcNow;
        DateTime accessExpiry = tokenService.GetAccessTokenExpiryUtc();
        DateTime refreshExpiry = tokenService.GetRefreshTokenExpiryUtc();

        accessExpiry.Should().BeOnOrAfter(before.AddMinutes(20).AddSeconds(-1));
        accessExpiry.Should().BeOnOrBefore(before.AddMinutes(20).AddSeconds(1));
        refreshExpiry.Should().BeOnOrAfter(before.AddDays(10).AddSeconds(-1));
        refreshExpiry.Should().BeOnOrBefore(before.AddDays(10).AddSeconds(1));
    }
}
