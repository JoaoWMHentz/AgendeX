using AgendeX.Application.Common.Interfaces;
using AgendeX.Application.Features.Auth.Common;
using AgendeX.Domain.Entities;
using AgendeX.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace AgendeX.Application.Features.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponseDto>;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
	private readonly IRefreshTokenRepository _refreshTokenRepository;
	private readonly ITokenService _tokenService;

	public RefreshTokenCommandHandler(
		IRefreshTokenRepository refreshTokenRepository,
		ITokenService tokenService)
	{
		_refreshTokenRepository = refreshTokenRepository;
		_tokenService = tokenService;
	}

	public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
	{
		AgendeX.Domain.Entities.RefreshToken refreshToken = await ValidateTokenAndGetAsync(request.RefreshToken, cancellationToken);
		refreshToken.IsRevoked = true;

		string newRefreshTokenValue = _tokenService.GenerateRefreshToken();
		string newRefreshTokenHash = _tokenService.ComputeSha256Hash(newRefreshTokenValue);

		AgendeX.Domain.Entities.RefreshToken rotatedToken = new()
		{
			Id = Guid.NewGuid(),
			UserId = refreshToken.UserId,
			TokenHash = newRefreshTokenHash,
			ExpiresAt = _tokenService.GetRefreshTokenExpiryUtc(),
			IsRevoked = false,
			CreatedAt = DateTime.UtcNow
		};

		await _refreshTokenRepository.AddAsync(rotatedToken, cancellationToken);
		await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

		return new AuthResponseDto
		{
			AccessToken = _tokenService.GenerateAccessToken(refreshToken.User),
			RefreshToken = newRefreshTokenValue,
			ExpiresAt = _tokenService.GetAccessTokenExpiryUtc()
		};
	}

	private async Task<AgendeX.Domain.Entities.RefreshToken> ValidateTokenAndGetAsync(string plainTextToken, CancellationToken cancellationToken)
	{
		string refreshTokenHash = _tokenService.ComputeSha256Hash(plainTextToken);

		AgendeX.Domain.Entities.RefreshToken? refreshToken = await _refreshTokenRepository.GetByTokenHashAsync(refreshTokenHash, cancellationToken);

		bool hasInvalidToken = refreshToken is null;
		bool isRevoked = refreshToken is not null && refreshToken.IsRevoked;
		bool isExpired = refreshToken is not null && refreshToken.ExpiresAt <= DateTime.UtcNow;
		bool userInactive = refreshToken is not null && !refreshToken.User.IsActive;

		if (hasInvalidToken || isRevoked || isExpired || userInactive)
		{
			throw new UnauthorizedAccessException("Invalid refresh token.");
		}

		return refreshToken!;
	}
}

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
	public RefreshTokenCommandValidator()
	{
		RuleFor(command => command.RefreshToken)
			.NotEmpty();
	}
}
