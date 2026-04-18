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
    public async Task Handle_ExistingToken_RevokesTokenAndPersistsChanges()
    {
        User user = new("Carlos", "carlos@email.com", "hash", UserRole.Client);

        RefreshToken refreshToken = new()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "logout-hash",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false,
            User = user
        };

        Mock<IRefreshTokenRepository> refreshTokenRepositoryMock = new();
        Mock<ITokenService> tokenServiceMock = new();

        tokenServiceMock
            .Setup(service => service.ComputeSha256Hash("logout-token"))
            .Returns("logout-hash");

        refreshTokenRepositoryMock
            .Setup(repository => repository.GetByTokenHashAsync("logout-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        LogoutCommandHandler handler = new(refreshTokenRepositoryMock.Object, tokenServiceMock.Object);

        await handler.Handle(new LogoutCommand("logout-token"), CancellationToken.None);

        refreshToken.IsRevoked.Should().BeTrue();

        refreshTokenRepositoryMock.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TokenNotFound_DoesNotPersistChanges()
    {
        Mock<IRefreshTokenRepository> refreshTokenRepositoryMock = new();
        Mock<ITokenService> tokenServiceMock = new();

        tokenServiceMock
            .Setup(service => service.ComputeSha256Hash("missing-token"))
            .Returns("missing-hash");

        refreshTokenRepositoryMock
            .Setup(repository => repository.GetByTokenHashAsync("missing-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        LogoutCommandHandler handler = new(refreshTokenRepositoryMock.Object, tokenServiceMock.Object);

        await handler.Handle(new LogoutCommand("missing-token"), CancellationToken.None);

        refreshTokenRepositoryMock.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
