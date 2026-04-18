using AgendeX.Application.Common.Interfaces;
using AgendeX.Domain.Entities;
using AgendeX.Domain.Interfaces;
using FluentValidation;
using MediatR;
using DomainRefreshToken = AgendeX.Domain.Entities.RefreshToken;

namespace AgendeX.Application.Features.Auth;

// ── Login ──────────────────────────────────────────────────────────────────

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

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.Password).NotEmpty().MinimumLength(6);
    }
}

// ── RefreshToken ───────────────────────────────────────────────────────────

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponseDto>;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(IRefreshTokenRepository refreshTokenRepository, ITokenService tokenService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        DomainRefreshToken refreshToken = await ValidateTokenAsync(request.RefreshToken, cancellationToken);
        refreshToken.IsRevoked = true;

        string newValue = _tokenService.GenerateRefreshToken();
        string newHash = _tokenService.ComputeSha256Hash(newValue);

        DomainRefreshToken rotated = new()
        {
            Id = Guid.NewGuid(),
            UserId = refreshToken.UserId,
            TokenHash = newHash,
            ExpiresAt = _tokenService.GetRefreshTokenExpiryUtc(),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.AddAsync(rotated, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = _tokenService.GenerateAccessToken(refreshToken.User),
            RefreshToken = newValue,
            ExpiresAt = _tokenService.GetAccessTokenExpiryUtc()
        };
    }

    private async Task<DomainRefreshToken> ValidateTokenAsync(string plainText, CancellationToken cancellationToken)
    {
        string hash = _tokenService.ComputeSha256Hash(plainText);
        DomainRefreshToken? token = await _refreshTokenRepository.GetByTokenHashAsync(hash, cancellationToken);

        if (token is null || token.IsRevoked || token.ExpiresAt <= DateTime.UtcNow || !token.User.IsActive)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        return token;
    }
}

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(c => c.RefreshToken).NotEmpty();
    }
}

// ── Logout ─────────────────────────────────────────────────────────────────

public sealed record LogoutCommand(string RefreshToken) : IRequest;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;

    public LogoutCommandHandler(IRefreshTokenRepository refreshTokenRepository, ITokenService tokenService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        string hash = _tokenService.ComputeSha256Hash(request.RefreshToken);
        DomainRefreshToken? token = await _refreshTokenRepository.GetByTokenHashAsync(hash, cancellationToken);

        if (token is null) return;

        token.IsRevoked = true;
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);
    }
}

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(c => c.RefreshToken).NotEmpty();
    }
}
