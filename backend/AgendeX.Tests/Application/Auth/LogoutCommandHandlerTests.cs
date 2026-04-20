using AgendeX.Application.Common.Interfaces;
using AgendeX.Application.Features.Auth;
using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace AgendeX.Tests.Application.Auth;

public sealed class LogoutCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingToken_RevokesToken()
    {
        Mock<IRefreshTokenRepository> refreshTokenRepository = new();
        Mock<ITokenService> tokenService = new();

        RefreshToken token = new()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TokenHash = "logout-hash",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            IsRevoked = false,
            User = new User("Carla", "carla@example.com", "password-hash", UserRole.Client)
        };

        tokenService.Setup(t => t.ComputeSha256Hash("logout-plain")).Returns("logout-hash");
        refreshTokenRepository
            .Setup(r => r.GetByTokenHashAsync("logout-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);
        refreshTokenRepository
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        LogoutCommandHandler handler = new(refreshTokenRepository.Object, tokenService.Object);

        await handler.Handle(new LogoutCommand("logout-plain"), CancellationToken.None);

        token.IsRevoked.Should().BeTrue();
        refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MissingToken_DoesNothing()
    {
        Mock<IRefreshTokenRepository> refreshTokenRepository = new();
        Mock<ITokenService> tokenService = new();

        tokenService.Setup(t => t.ComputeSha256Hash("logout-plain")).Returns("logout-hash");
        refreshTokenRepository
            .Setup(r => r.GetByTokenHashAsync("logout-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        LogoutCommandHandler handler = new(refreshTokenRepository.Object, tokenService.Object);

        await handler.Handle(new LogoutCommand("logout-plain"), CancellationToken.None);

        refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}