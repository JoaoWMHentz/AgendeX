using AgendeX.Domain.Entities;
using AgendeX.Domain.Interfaces;
using MediatR;

namespace AgendeX.Application.Features.Users;

public sealed record DeleteUserCommand(Guid Id) : IRequest;

public sealed class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserRepository _userRepository;

    public DeleteUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        User user = await _userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"User '{request.Id}' not found.");

        user.Deactivate();
        await _userRepository.SaveChangesAsync(cancellationToken);
    }
}
