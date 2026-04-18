using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using MediatR;

namespace AgendeX.Application.Features.Users;

// ── GetUsers ───────────────────────────────────────────────────────────────

public sealed record GetUsersQuery(UserRole? Role) : IRequest<IReadOnlyList<UserDto>>;

public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IReadOnlyList<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<User> users = await _userRepository.GetAllAsync(request.Role, cancellationToken);
        return users.Select(UserMapper.ToDto).ToList().AsReadOnly();
    }
}

// ── GetUserById ────────────────────────────────────────────────────────────

public sealed record GetUserByIdQuery(Guid Id) : IRequest<UserDto>;

public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        User user = await _userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"User '{request.Id}' not found.");

        return UserMapper.ToDto(user);
    }
}
