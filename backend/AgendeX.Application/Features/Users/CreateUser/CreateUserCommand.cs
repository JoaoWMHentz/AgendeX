using AgendeX.Domain.Enums;
using MediatR;

namespace AgendeX.Application.Features.Users;

public sealed record CreateUserCommand(
    string Name,
    string Email,
    string Password,
    UserRole Role
) : IRequest<UserDto>;
