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
    UserRole Role,
    string? CPF,
    DateOnly? BirthDate,
    string? Phone,
    string? Notes
) : IRequest<UserDto>;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IClientDetailRepository _clientDetailRepository;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IClientDetailRepository clientDetailRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _clientDetailRepository = clientDetailRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        await EnsureEmailIsUniqueAsync(request.Email, cancellationToken);

        User user = new(request.Name, request.Email, _passwordHasher.Hash(request.Password), request.Role);

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        if (request.Role == UserRole.Client)
            await CreateClientDetailAsync(user.Id, request, cancellationToken);

        User created = await _userRepository.GetByIdAsync(user.Id, cancellationToken) ?? user;
        return UserMapper.ToDto(created);
    }

    private async Task EnsureEmailIsUniqueAsync(string email, CancellationToken cancellationToken)
    {
        User? existing = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException($"Email '{email}' is already in use.");
    }

    private async Task CreateClientDetailAsync(Guid userId, CreateUserCommand request, CancellationToken cancellationToken)
    {
        ClientDetail detail = new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CPF = request.CPF!,
            BirthDate = request.BirthDate!.Value,
            Phone = request.Phone!,
            Notes = request.Notes
        };

        await _clientDetailRepository.AddAsync(detail, cancellationToken);
        await _clientDetailRepository.SaveChangesAsync(cancellationToken);
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

        When(c => c.Role == UserRole.Client, () =>
        {
            RuleFor(c => c.CPF).NotEmpty().MaximumLength(14);
            RuleFor(c => c.BirthDate).NotNull();
            RuleFor(c => c.Phone).NotEmpty().MaximumLength(20);
        });
    }
}

// ── UpdateUser ─────────────────────────────────────────────────────────────

public sealed record UpdateUserCommand(
    Guid Id,
    string Name,
    string? CPF,
    DateOnly? BirthDate,
    string? Phone,
    string? Notes
) : IRequest<UserDto>;

public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IClientDetailRepository _clientDetailRepository;

    public UpdateUserCommandHandler(IUserRepository userRepository, IClientDetailRepository clientDetailRepository)
    {
        _userRepository = userRepository;
        _clientDetailRepository = clientDetailRepository;
    }

    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        User user = await _userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"User '{request.Id}' not found.");

        user.Update(request.Name);
        await _userRepository.SaveChangesAsync(cancellationToken);

        if (user.Role == UserRole.Client)
            await UpdateClientDetailAsync(user.Id, request, cancellationToken);

        User updated = await _userRepository.GetByIdAsync(user.Id, cancellationToken) ?? user;
        return UserMapper.ToDto(updated);
    }

    private async Task UpdateClientDetailAsync(Guid userId, UpdateUserCommand request, CancellationToken cancellationToken)
    {
        ClientDetail? detail = await _clientDetailRepository.GetByUserIdAsync(userId, cancellationToken);
        if (detail is null) return;

        detail.Update(
            request.CPF ?? detail.CPF,
            request.BirthDate ?? detail.BirthDate,
            request.Phone ?? detail.Phone,
            request.Notes);

        await _clientDetailRepository.SaveChangesAsync(cancellationToken);
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
