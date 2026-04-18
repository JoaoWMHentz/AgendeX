using AgendeX.Application.Common.Interfaces;
using AgendeX.Application.Features.Auth.Commands.Login;
using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace AgendeX.Tests.Application.Auth;

public sealed class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCredentials_ReturnsAuthResponseAndPersistsRefreshToken()
    {
        User user = new("Maria", "maria@email.com", "hashed-password", UserRole.Client);

        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IRefreshTokenRepository> refreshTokenRepositoryMock = new();
        Mock<IPasswordHasher> passwordHasherMock = new();
        Mock<ITokenService> tokenServiceMock = new();

        userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync("maria@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        passwordHasherMock
            .Setup(hasher => hasher.Verify("123456", "hashed-password"))
            .Returns(true);

        tokenServiceMock
            .Setup(service => service.GenerateRefreshToken())
            .Returns("plain-refresh-token");

        tokenServiceMock
            .Setup(service => service.ComputeSha256Hash("plain-refresh-token"))
            .Returns("hashed-refresh-token");

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
            .Returns("access-token");

        RefreshToken? persistedToken = null;

        refreshTokenRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Callback<RefreshToken, CancellationToken>((token, _) => persistedToken = token)
            .Returns(Task.CompletedTask);

        LoginCommandHandler handler = new(
            userRepositoryMock.Object,
            refreshTokenRepositoryMock.Object,
            passwordHasherMock.Object,
            tokenServiceMock.Object);

        LoginCommand command = new("maria@email.com", "123456");

        var result = await handler.Handle(command, CancellationToken.None);

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("plain-refresh-token");
        result.ExpiresAt.Should().Be(accessExpiry);

        persistedToken.Should().NotBeNull();
        persistedToken!.UserId.Should().Be(user.Id);
        persistedToken.TokenHash.Should().Be("hashed-refresh-token");
        persistedToken.ExpiresAt.Should().Be(refreshExpiry);
        persistedToken.IsRevoked.Should().BeFalse();

        refreshTokenRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        refreshTokenRepositoryMock.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsUnauthorizedAccessException()
    {
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IRefreshTokenRepository> refreshTokenRepositoryMock = new();
        Mock<IPasswordHasher> passwordHasherMock = new();
        Mock<ITokenService> tokenServiceMock = new();

        userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        LoginCommandHandler handler = new(
            userRepositoryMock.Object,
            refreshTokenRepositoryMock.Object,
            passwordHasherMock.Object,
            tokenServiceMock.Object);

        Func<Task> action = async () => await handler.Handle(new LoginCommand("missing@email.com", "123456"), CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials.");

        refreshTokenRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
        refreshTokenRepositoryMock.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserInactive_ThrowsUnauthorizedAccessException()
    {
        User inactiveUser = new("Maria", "maria@email.com", "hashed-password", UserRole.Client);
        inactiveUser.Deactivate();

        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IRefreshTokenRepository> refreshTokenRepositoryMock = new();
        Mock<IPasswordHasher> passwordHasherMock = new();
        Mock<ITokenService> tokenServiceMock = new();

        userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveUser);

        LoginCommandHandler handler = new(
            userRepositoryMock.Object,
            refreshTokenRepositoryMock.Object,
            passwordHasherMock.Object,
            tokenServiceMock.Object);

        Func<Task> action = async () => await handler.Handle(new LoginCommand("maria@email.com", "123456"), CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials.");
    }

    [Fact]
    public async Task Handle_InvalidPassword_ThrowsUnauthorizedAccessException()
    {
        User user = new("Maria", "maria@email.com", "hashed-password", UserRole.Client);

        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IRefreshTokenRepository> refreshTokenRepositoryMock = new();
        Mock<IPasswordHasher> passwordHasherMock = new();
        Mock<ITokenService> tokenServiceMock = new();

        userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        passwordHasherMock
            .Setup(hasher => hasher.Verify("wrong-password", "hashed-password"))
            .Returns(false);

        LoginCommandHandler handler = new(
            userRepositoryMock.Object,
            refreshTokenRepositoryMock.Object,
            passwordHasherMock.Object,
            tokenServiceMock.Object);

        Func<Task> action = async () => await handler.Handle(new LoginCommand("maria@email.com", "wrong-password"), CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials.");

        refreshTokenRepositoryMock.Verify(repository => repository.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
        refreshTokenRepositoryMock.Verify(repository => repository.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
