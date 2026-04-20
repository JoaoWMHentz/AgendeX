using AgendeX.Application.Common.Interfaces;
using AgendeX.Application.Features.Auth;
using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace AgendeX.Tests.Application.Auth;

public sealed class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCredentials_ReturnsTokensAndPersistsRefreshToken()
    {
        Mock<IUserRepository> userRepository = new();
        Mock<IRefreshTokenRepository> refreshTokenRepository = new();
        Mock<IPasswordHasher> passwordHasher = new();
        Mock<ITokenService> tokenService = new();

        User user = new("Ana", "ana@example.com", "password-hash", UserRole.Administrator);

        userRepository
            .Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        passwordHasher
            .Setup(p => p.Verify("Secret123", user.PasswordHash))
            .Returns(true);

        tokenService.Setup(t => t.GenerateRefreshToken()).Returns("refresh-token");
        tokenService.Setup(t => t.ComputeSha256Hash("refresh-token")).Returns("refresh-token-hash");
        tokenService.Setup(t => t.GetRefreshTokenExpiryUtc()).Returns(new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        tokenService.Setup(t => t.GenerateAccessToken(user)).Returns("access-token");
        tokenService.Setup(t => t.GetAccessTokenExpiryUtc()).Returns(new DateTime(2030, 1, 1, 1, 0, 0, DateTimeKind.Utc));

        refreshTokenRepository
            .Setup(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        refreshTokenRepository
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        LoginCommandHandler handler = new(userRepository.Object, refreshTokenRepository.Object, passwordHasher.Object, tokenService.Object);

        AuthResponseDto result = await handler.Handle(new LoginCommand(user.Email, "Secret123"), CancellationToken.None);

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.ExpiresAt.Should().Be(new DateTime(2030, 1, 1, 1, 0, 0, DateTimeKind.Utc));

        refreshTokenRepository.Verify(r => r.AddAsync(
            It.Is<RefreshToken>(token =>
                token.UserId == user.Id &&
                token.TokenHash == "refresh-token-hash" &&
                token.ExpiresAt == new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc) &&
                !token.IsRevoked),
            It.IsAny<CancellationToken>()), Times.Once);
        refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidPassword_ThrowsUnauthorizedAccessException()
    {
        Mock<IUserRepository> userRepository = new();
        Mock<IRefreshTokenRepository> refreshTokenRepository = new();
        Mock<IPasswordHasher> passwordHasher = new();
        Mock<ITokenService> tokenService = new();

        User user = new("Ana", "ana@example.com", "password-hash", UserRole.Administrator);

        userRepository
            .Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        passwordHasher
            .Setup(p => p.Verify("Wrong123", user.PasswordHash))
            .Returns(false);

        LoginCommandHandler handler = new(userRepository.Object, refreshTokenRepository.Object, passwordHasher.Object, tokenService.Object);

        Func<Task> act = () => handler.Handle(new LoginCommand(user.Email, "Wrong123"), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials.");

        refreshTokenRepository.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
        refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}