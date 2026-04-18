using AgendeX.Application.Common.Interfaces;
using AgendeX.Domain.Entities;
using AgendeX.Domain.Interfaces;
using MediatR;
using DomainRefreshToken = AgendeX.Domain.Entities.RefreshToken;

namespace AgendeX.Application.Features.Auth;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        User user = await ValidateCredentialsAsync(request, cancellationToken);
        return await CreateAuthResponseAsync(user, cancellationToken);
    }

    private async Task<User> ValidateCredentialsAsync(LoginCommand request, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        bool isInvalidUser = user is null || !user.IsActive;
        bool isInvalidPassword = user is not null && !_passwordHasher.Verify(request.Password, user.PasswordHash);

        if (isInvalidUser || isInvalidPassword)
            throw new UnauthorizedAccessException("Invalid credentials.");

        return user!;
    }

    private async Task<AuthResponseDto> CreateAuthResponseAsync(User user, CancellationToken cancellationToken)
    {
        string refreshTokenValue = _tokenService.GenerateRefreshToken();
        string refreshTokenHash = _tokenService.ComputeSha256Hash(refreshTokenValue);

        DomainRefreshToken refreshToken = new()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = _tokenService.GetRefreshTokenExpiryUtc(),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = _tokenService.GenerateAccessToken(user),
            RefreshToken = refreshTokenValue,
            ExpiresAt = _tokenService.GetAccessTokenExpiryUtc()
        };
    }
}
