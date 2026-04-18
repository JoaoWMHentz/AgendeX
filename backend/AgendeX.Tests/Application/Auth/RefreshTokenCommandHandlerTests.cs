using AgendeX.Application.Common.Interfaces;
using AgendeX.Application.Features.Auth;
using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace AgendeX.Tests.Application.Auth;

public sealed class RefreshTokenCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidToken_RotatesTokenAndReturnsAuthResponse()
    {
        User user = new("Ana", "ana@email.com", "hash", UserRole.Client);

        RefreshToken currentToken = new()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "old-hash",
            ExpiresAt = DateTime.UtcNow.AddDays(3),
            IsRevoked = false,
            User = user
        };

        Mock<IRefreshTokenRepository> refreshTokenRepositoryMock = new();
        Mock<ITokenService> tokenServiceMock = new();

        refreshTokenRepositoryMock
            .Setup(repository => repository.GetByTokenHashAsync("old-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentToken);

        tokenServiceMock
            .Setup(service => service.ComputeSha256Hash("old-plain-token"))
            .Returns("old-hash");

        tokenServiceMock
            .Setup(service => service.GenerateRefreshToken())
            .Returns("new-plain-token");

        tokenServiceMock
            .Setup(service => service.ComputeSha256Hash("new-plain-token"))
            .Returns("new-hash");

        DateTime refreshExpiry = DateTime.UtcNow.AddDays(7);
        tokenServiceMock
            .Setup(service => service.GetRefreshTokenExpiryUtc())
            .Returns(refreshExpiry);

        DateTime accessExpiry = DateTime.UtcNow.AddMinutes(15);
        tokenServiceMock
            .Setup(service => service.GetAccessTokenExpiryUtc())
            .Returns(accessExpiry);

        tokenServiceMock
            .Setup(service => service.GenerateAccessToken(user))
            .Returns("new-access-token");

        RefreshToken? rotatedToken = null;

        refreshTokenRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Callback<RefreshToken, CancellationToken>((token, _) => rotatedToken = token)
            .Returns(Task.CompletedTask);

        RefreshTokenCommandHandler handler = new(refreshTokenRepositoryMock.Object, tokenServiceMock.Object);

        var result = await handler.Handle(new RefreshTokenCommand("old-plain-token"), CancellationToken.None);

        currentToken.IsRevoked.Should().BeTrue();

        rotatedToken.Should().NotBeNull();
        rotatedToken!.UserId.Should().Be(user.Id);
        rotatedToken.TokenHash.Should().Be("new-hash");
        rotatedToken.IsRevoked.Should().BeFalse();
        rotatedToken.ExpiresAt.Should().Be(refreshExpiry);

        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-plain-token");
        result.ExpiresAt.Should().Be(accessExpiry);

        refreshTokenRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        refreshTokenRepositoryMock.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TokenNotFound_ThrowsUnauthorizedAccessException()
    {
        Mock<IRefreshTokenRepository> refreshTokenRepositoryMock = new();
        Mock<ITokenService> tokenServiceMock = new();

        tokenServiceMock
            .Setup(service => service.ComputeSha256Hash("missing-token"))
            .Returns("missing-hash");

        refreshTokenRepositoryMock
            .Setup(repository => repository.GetByTokenHashAsync("missing-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        RefreshTokenCommandHandler handler = new(refreshTokenRepositoryMock.Object, tokenServiceMock.Object);

        Func<Task> action = async () => await handler.Handle(new RefreshTokenCommand("missing-token"), CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid refresh token.");
    }

    [Fact]
    public async Task Handle_RevokedToken_ThrowsUnauthorizedAccessException()
    {
        User user = new("Ana", "ana@email.com", "hash", UserRole.Client);

        RefreshToken revokedToken = new()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "revoked-hash",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = true,
            User = user
        };

        Mock<IRefreshTokenRepository> refreshTokenRepositoryMock = new();
        Mock<ITokenService> tokenServiceMock = new();

        tokenServiceMock
            .Setup(service => service.ComputeSha256Hash("revoked-token"))
            .Returns("revoked-hash");

        refreshTokenRepositoryMock
            .Setup(repository => repository.GetByTokenHashAsync("revoked-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(revokedToken);

        RefreshTokenCommandHandler handler = new(refreshTokenRepositoryMock.Object, tokenServiceMock.Object);

        Func<Task> action = async () => await handler.Handle(new RefreshTokenCommand("revoked-token"), CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid refresh token.");
    }

    [Fact]
    public async Task Handle_ExpiredToken_ThrowsUnauthorizedAccessException()
    {
        User user = new("Ana", "ana@email.com", "hash", UserRole.Client);

        RefreshToken expiredToken = new()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "expired-hash",
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
            IsRevoked = false,
            User = user
        };

        Mock<IRefreshTokenRepository> refreshTokenRepositoryMock = new();
        Mock<ITokenService> tokenServiceMock = new();

        tokenServiceMock
            .Setup(service => service.ComputeSha256Hash("expired-token"))
            .Returns("expired-hash");

        refreshTokenRepositoryMock
            .Setup(repository => repository.GetByTokenHashAsync("expired-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredToken);

        RefreshTokenCommandHandler handler = new(refreshTokenRepositoryMock.Object, tokenServiceMock.Object);

        Func<Task> action = async () => await handler.Handle(new RefreshTokenCommand("expired-token"), CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid refresh token.");
    }

    [Fact]
    public async Task Handle_InactiveUser_ThrowsUnauthorizedAccessException()
    {
        User inactiveUser = new("Ana", "ana@email.com", "hash", UserRole.Client);
        inactiveUser.Deactivate();

        RefreshToken refreshToken = new()
        {
            Id = Guid.NewGuid(),
            UserId = inactiveUser.Id,
            TokenHash = "inactive-user-hash",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false,
            User = inactiveUser
        };

        Mock<IRefreshTokenRepository> refreshTokenRepositoryMock = new();
        Mock<ITokenService> tokenServiceMock = new();

        tokenServiceMock
            .Setup(service => service.ComputeSha256Hash("inactive-user-token"))
            .Returns("inactive-user-hash");

        refreshTokenRepositoryMock
            .Setup(repository => repository.GetByTokenHashAsync("inactive-user-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        RefreshTokenCommandHandler handler = new(refreshTokenRepositoryMock.Object, tokenServiceMock.Object);

        Func<Task> action = async () => await handler.Handle(new RefreshTokenCommand("inactive-user-token"), CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid refresh token.");
    }
}
