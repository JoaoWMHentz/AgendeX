using AgendeX.Application.Common.Interfaces;
using AgendeX.Domain.Interfaces;
using FluentValidation;
using MediatR;
using DomainRefreshToken = AgendeX.Domain.Entities.RefreshToken;

namespace AgendeX.Application.Features.Auth.Commands.Logout;

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
		string refreshTokenHash = _tokenService.ComputeSha256Hash(request.RefreshToken);

		DomainRefreshToken? refreshToken = await _refreshTokenRepository.GetByTokenHashAsync(refreshTokenHash, cancellationToken);

		if (refreshToken is null)
		{
			return;
		}

		refreshToken.IsRevoked = true;
		await _refreshTokenRepository.SaveChangesAsync(cancellationToken);
	}
}

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
	public LogoutCommandValidator()
	{
		RuleFor(command => command.RefreshToken)
			.NotEmpty();
	}
}
