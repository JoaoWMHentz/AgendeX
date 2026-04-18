using AgendeX.Application.Common.Interfaces;
using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace AgendeX.Application.Features.Users;

// ── CreateUser ─────────────────────────────────────────────────────────────

public sealed record CreateUserCommand(
    string Name,
    string Email,
    string Password,
    UserRole Role
) : IRequest<UserDto>;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        User? existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException($"Email '{request.Email}' is already in use.");

        User user = new(request.Name, request.Email, _passwordHasher.Hash(request.Password), request.Role);

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return UserMapper.ToDto(user);
    }
}

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(120);
        RuleFor(c => c.Email).NotEmpty().EmailAddress().MaximumLength(180);
        RuleFor(c => c.Password).NotEmpty().MinimumLength(6);
        RuleFor(c => c.Role).IsInEnum();
    }
}

// ── SetClientDetail ─────────────────────────────────────────────────────────

public sealed record SetClientDetailCommand(
    Guid UserId,
    string CPF,
    DateOnly BirthDate,
    string Phone,
    string? Notes
) : IRequest<UserDto>;

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

public sealed class SetClientDetailCommandValidator : AbstractValidator<SetClientDetailCommand>
{
    public SetClientDetailCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.CPF).NotEmpty().MaximumLength(14);
        RuleFor(c => c.BirthDate).NotEmpty();
        RuleFor(c => c.Phone).NotEmpty().MaximumLength(20);
    }
}

// ── UpdateUser ─────────────────────────────────────────────────────────────

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

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(120);
    }
}

// ── DeleteUser ─────────────────────────────────────────────────────────────

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

public sealed class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}
