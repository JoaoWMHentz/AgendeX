using AgendeX.Application.Common.Interfaces;
using AgendeX.Application.Features.Auth.Commands.Login;
using AgendeX.Application.Features.Auth.Commands.Logout;
using AgendeX.Application.Features.Auth.Commands.RefreshToken;
using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using FluentAssertions;

namespace AgendeX.Tests.Application.Auth;

public sealed class AuthFlowTests
{
    [Fact]
    public async Task FullAuthFlow_CreateUser_Login_Refresh_Logout_WorksAsExpected()
    {
        FakeUserRepository userRepository = new();
        FakeRefreshTokenRepository refreshTokenRepository = new(userRepository);
        FakePasswordHasher passwordHasher = new();
        FakeTokenService tokenService = new();

        User user = new("Joao", "joao@email.com", passwordHasher.Hash("123456"), UserRole.Client);
        userRepository.Add(user);

        LoginCommandHandler loginHandler = new(userRepository, refreshTokenRepository, passwordHasher, tokenService);
        RefreshTokenCommandHandler refreshHandler = new(refreshTokenRepository, tokenService);
        LogoutCommandHandler logoutHandler = new(refreshTokenRepository, tokenService);

        var loginResponse = await loginHandler.Handle(new LoginCommand("joao@email.com", "123456"), CancellationToken.None);

        loginResponse.AccessToken.Should().StartWith("access-");
        loginResponse.RefreshToken.Should().Be("refresh-1");

        RefreshToken loginToken = refreshTokenRepository.FindByPlainToken(loginResponse.RefreshToken)!;
        loginToken.IsRevoked.Should().BeFalse();

        var refreshResponse = await refreshHandler.Handle(new RefreshTokenCommand(loginResponse.RefreshToken), CancellationToken.None);

        refreshResponse.AccessToken.Should().StartWith("access-");
        refreshResponse.RefreshToken.Should().Be("refresh-2");

        loginToken.IsRevoked.Should().BeTrue();

        RefreshToken rotatedToken = refreshTokenRepository.FindByPlainToken(refreshResponse.RefreshToken)!;
        rotatedToken.IsRevoked.Should().BeFalse();

        await logoutHandler.Handle(new LogoutCommand(refreshResponse.RefreshToken), CancellationToken.None);

        rotatedToken.IsRevoked.Should().BeTrue();
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly Dictionary<string, User> _usersByEmail = new(StringComparer.OrdinalIgnoreCase);

        public void Add(User user)
        {
            _usersByEmail[user.Email] = user;
        }

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        {
            _usersByEmail.TryGetValue(email, out User? user);
            return Task.FromResult(user);
        }

        public User? FindById(Guid userId)
        {
            return _usersByEmail.Values.FirstOrDefault(user => user.Id == userId);
        }
    }

    private sealed class FakeRefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly FakeUserRepository _userRepository;
        private readonly List<RefreshToken> _refreshTokens = [];

        public FakeRefreshTokenRepository(FakeUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken)
        {
            RefreshToken? token = _refreshTokens.FirstOrDefault(refreshToken => refreshToken.TokenHash == tokenHash);

            if (token is not null)
            {
                token.User = _userRepository.FindById(token.UserId)!;
            }

            return Task.FromResult(token);
        }

        public Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
        {
            _refreshTokens.Add(refreshToken);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public RefreshToken? FindByPlainToken(string plainToken)
        {
            string hash = $"sha-{plainToken}";
            return _refreshTokens.FirstOrDefault(token => token.TokenHash == hash);
        }
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            return $"hash::{password}";
        }

        public bool Verify(string password, string passwordHash)
        {
            return passwordHash == Hash(password);
        }
    }

    private sealed class FakeTokenService : ITokenService
    {
        private int _accessCounter;
        private int _refreshCounter;

        public string GenerateAccessToken(User user)
        {
            _accessCounter++;
            return $"access-{user.Id:N}-{_accessCounter}";
        }

        public string GenerateRefreshToken()
        {
            _refreshCounter++;
            return $"refresh-{_refreshCounter}";
        }

        public string ComputeSha256Hash(string value)
        {
            return $"sha-{value}";
        }

        public DateTime GetAccessTokenExpiryUtc()
        {
            return DateTime.UtcNow.AddMinutes(15);
        }

        public DateTime GetRefreshTokenExpiryUtc()
        {
            return DateTime.UtcNow.AddDays(7);
        }
    }
}
