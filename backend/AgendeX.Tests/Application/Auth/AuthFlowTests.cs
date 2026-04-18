using AgendeX.Application.Common.Interfaces;
using AgendeX.Application.Features.Auth;
using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace AgendeX.Tests.Application.Auth;

public sealed class AuthFlowTests
{
    private readonly User _user = new("Joao", "joao@email.com", "hashed-password", UserRole.Client);

    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepo = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<ITokenService> _tokenService = new();

    private readonly List<RefreshToken> _storedTokens = [];

    public AuthFlowTests()
    {
        _userRepo
            .Setup(r => r.GetByEmailAsync("joao@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(_user);

        _passwordHasher
            .Setup(h => h.Verify("123456", "hashed-password"))
            .Returns(true);

        _tokenService
            .SetupSequence(s => s.GenerateRefreshToken())
            .Returns("plain-token-1")
            .Returns("plain-token-2");

        _tokenService.Setup(s => s.ComputeSha256Hash("plain-token-1")).Returns("hash-1");
        _tokenService.Setup(s => s.ComputeSha256Hash("plain-token-2")).Returns("hash-2");
        _tokenService.Setup(s => s.GenerateAccessToken(_user)).Returns("access-token");
        _tokenService.Setup(s => s.GetAccessTokenExpiryUtc()).Returns(DateTime.UtcNow.AddMinutes(15));
        _tokenService.Setup(s => s.GetRefreshTokenExpiryUtc()).Returns(DateTime.UtcNow.AddDays(7));

        _refreshTokenRepo
            .Setup(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Callback<RefreshToken, CancellationToken>((token, _) =>
            {
                token.User = _user;
                _storedTokens.Add(token);
            })
            .Returns(Task.CompletedTask);

        _refreshTokenRepo
            .Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string hash, CancellationToken _) => _storedTokens.FirstOrDefault(t => t.TokenHash == hash));
    }

    [Fact]
    public async Task FullAuthFlow_Login_ReturnsValidTokens()
    {
        LoginCommandHandler handler = new(_userRepo.Object, _refreshTokenRepo.Object, _passwordHasher.Object, _tokenService.Object);

        AuthResponseDto result = await handler.Handle(new LoginCommand("joao@email.com", "123456"), CancellationToken.None);

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("plain-token-1");
        _storedTokens.Should().ContainSingle(t => t.TokenHash == "hash-1" && !t.IsRevoked);
    }

    [Fact]
    public async Task FullAuthFlow_Refresh_RotatesTokenAndRevokesOld()
    {
        LoginCommandHandler loginHandler = new(_userRepo.Object, _refreshTokenRepo.Object, _passwordHasher.Object, _tokenService.Object);
        RefreshTokenCommandHandler refreshHandler = new(_refreshTokenRepo.Object, _tokenService.Object);

        await loginHandler.Handle(new LoginCommand("joao@email.com", "123456"), CancellationToken.None);

        AuthResponseDto result = await refreshHandler.Handle(new RefreshTokenCommand("plain-token-1"), CancellationToken.None);

        result.RefreshToken.Should().Be("plain-token-2");
        _storedTokens.First(t => t.TokenHash == "hash-1").IsRevoked.Should().BeTrue();
        _storedTokens.First(t => t.TokenHash == "hash-2").IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task FullAuthFlow_Logout_RevokesActiveToken()
    {
        LoginCommandHandler loginHandler = new(_userRepo.Object, _refreshTokenRepo.Object, _passwordHasher.Object, _tokenService.Object);
        RefreshTokenCommandHandler refreshHandler = new(_refreshTokenRepo.Object, _tokenService.Object);
        LogoutCommandHandler logoutHandler = new(_refreshTokenRepo.Object, _tokenService.Object);

        await loginHandler.Handle(new LoginCommand("joao@email.com", "123456"), CancellationToken.None);
        AuthResponseDto refreshResult = await refreshHandler.Handle(new RefreshTokenCommand("plain-token-1"), CancellationToken.None);

        await logoutHandler.Handle(new LogoutCommand(refreshResult.RefreshToken), CancellationToken.None);

        _storedTokens.Should().OnlyContain(t => t.IsRevoked);
    }
}
