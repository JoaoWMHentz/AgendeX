using AgendeX.Domain.Entities;
using AgendeX.Domain.Interfaces;
using MediatR;

namespace AgendeX.Application.Features.Users;

public sealed record UpdateUserCommand(Guid Id, string Name) : IRequest<UserDto>;

public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        User user = await _userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"User '{request.Id}' not found.");

        user.Update(request.Name);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return UserMapper.ToDto(user);
    }
}
