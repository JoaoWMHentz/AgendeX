using AgendeX.Application.Common.Interfaces;
using AgendeX.Domain.Interfaces;
using MediatR;
using DomainRefreshToken = AgendeX.Domain.Entities.RefreshToken;

namespace AgendeX.Application.Features.Auth;

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
