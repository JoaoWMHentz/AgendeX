using AgendeX.Application.Common.Interfaces;
using AgendeX.Domain.Interfaces;
using MediatR;
using DomainRefreshToken = AgendeX.Domain.Entities.RefreshToken;

namespace AgendeX.Application.Features.Auth;

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
