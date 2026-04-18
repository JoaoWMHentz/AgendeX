using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using MediatR;

namespace AgendeX.Application.Features.Users;

public sealed class SetClientDetailCommandHandler : IRequestHandler<SetClientDetailCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IClientDetailRepository _clientDetailRepository;

    public SetClientDetailCommandHandler(
        IUserRepository userRepository,
        IClientDetailRepository clientDetailRepository)
    {
        _userRepository = userRepository;
        _clientDetailRepository = clientDetailRepository;
    }

    public async Task<UserDto> Handle(SetClientDetailCommand request, CancellationToken cancellationToken)
    {
        User user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"User '{request.UserId}' not found.");

        if (user.Role != UserRole.Client)
            throw new InvalidOperationException("Client details can only be set for users with role 'Client'.");

        ClientDetail? existing = await _clientDetailRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (existing is null)
        {
            ClientDetail detail = new()
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                CPF = request.CPF,
                BirthDate = request.BirthDate,
                Phone = request.Phone,
                Notes = request.Notes
            };
            await _clientDetailRepository.AddAsync(detail, cancellationToken);
        }
        else
        {
            existing.Update(request.CPF, request.BirthDate, request.Phone, request.Notes);
        }

        await _clientDetailRepository.SaveChangesAsync(cancellationToken);

        User updated = await _userRepository.GetByIdAsync(user.Id, cancellationToken) ?? user;
        return UserMapper.ToDto(updated);
    }
}
