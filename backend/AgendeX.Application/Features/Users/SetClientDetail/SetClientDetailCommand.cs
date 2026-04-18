using MediatR;

namespace AgendeX.Application.Features.Users;

public sealed record SetClientDetailRequest(
    string CPF,
    DateOnly BirthDate,
    string Phone,
    string? Notes);

public sealed record SetClientDetailCommand(
    Guid UserId,
    string CPF,
    DateOnly BirthDate,
    string Phone,
    string? Notes
) : IRequest<UserDto>;
