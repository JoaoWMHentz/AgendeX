using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;

namespace AgendeX.Application.Features.Users;

public sealed record UserDto(
    Guid Id,
    string Name,
    string Email,
    UserRole Role,
    bool IsActive,
    DateTime CreatedAt,
    ClientDetailDto? ClientDetail
);

public sealed record ClientDetailDto(
    Guid Id,
    string CPF,
    DateOnly BirthDate,
    string Phone,
    string? Notes
);

internal static class UserMapper
{
    internal static UserDto ToDto(User user) => new(
        user.Id,
        user.Name,
        user.Email,
        user.Role,
        user.IsActive,
        user.CreatedAt,
        user.ClientDetail is null ? null : new ClientDetailDto(
            user.ClientDetail.Id,
            user.ClientDetail.CPF,
            user.ClientDetail.BirthDate,
            user.ClientDetail.Phone,
            user.ClientDetail.Notes
        )
    );
}
