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
    public async Task Handle_ValidToken_RotatesTokenAndReturnsNewTokens()
    {
        Mock<IRefreshTokenRepository> refreshTokenRepository = new();
        Mock<ITokenService> tokenService = new();

        User user = new("Bruno", "bruno@example.com", "password-hash", UserRole.Agent);
        RefreshToken currentToken = new()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "current-hash",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            IsRevoked = false,
            User = user
        };

        tokenService.Setup(t => t.ComputeSha256Hash("current-plain")).Returns("current-hash");
        tokenService.Setup(t => t.GenerateRefreshToken()).Returns("rotated-plain");
        tokenService.Setup(t => t.ComputeSha256Hash("rotated-plain")).Returns("rotated-hash");
        tokenService.Setup(t => t.GetRefreshTokenExpiryUtc()).Returns(new DateTime(2030, 2, 1, 0, 0, 0, DateTimeKind.Utc));
        tokenService.Setup(t => t.GenerateAccessToken(user)).Returns("rotated-access");
        tokenService.Setup(t => t.GetAccessTokenExpiryUtc()).Returns(new DateTime(2030, 2, 1, 1, 0, 0, DateTimeKind.Utc));

        refreshTokenRepository
            .Setup(r => r.GetByTokenHashAsync("current-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentToken);
        refreshTokenRepository
            .Setup(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        refreshTokenRepository
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        RefreshTokenCommandHandler handler = new(refreshTokenRepository.Object, tokenService.Object);

        AuthResponseDto result = await handler.Handle(new RefreshTokenCommand("current-plain"), CancellationToken.None);

        result.AccessToken.Should().Be("rotated-access");
        result.RefreshToken.Should().Be("rotated-plain");
        result.ExpiresAt.Should().Be(new DateTime(2030, 2, 1, 1, 0, 0, DateTimeKind.Utc));
        currentToken.IsRevoked.Should().BeTrue();

        refreshTokenRepository.Verify(r => r.AddAsync(
            It.Is<RefreshToken>(token =>
                token.UserId == user.Id &&
                token.TokenHash == "rotated-hash" &&
                token.ExpiresAt == new DateTime(2030, 2, 1, 0, 0, 0, DateTimeKind.Utc) &&
                !token.IsRevoked),
            It.IsAny<CancellationToken>()), Times.Once);
        refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MissingToken_ThrowsUnauthorizedAccessException()
    {
        Mock<IRefreshTokenRepository> refreshTokenRepository = new();
        Mock<ITokenService> tokenService = new();

        tokenService.Setup(t => t.ComputeSha256Hash("missing-plain")).Returns("missing-hash");
        refreshTokenRepository
            .Setup(r => r.GetByTokenHashAsync("missing-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        RefreshTokenCommandHandler handler = new(refreshTokenRepository.Object, tokenService.Object);

        Func<Task> act = () => handler.Handle(new RefreshTokenCommand("missing-plain"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid refresh token.");

        refreshTokenRepository.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
        refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}