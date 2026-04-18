using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace AgendeX.Application.Features.Availability;

// ── CreateAvailability ──────────────────────────────────────────────────────

public sealed record CreateAvailabilityCommand(
    Guid AgentId,
    WeekDay WeekDay,
    TimeOnly StartTime,
    TimeOnly EndTime
) : IRequest<AvailabilityDto>;

public sealed class CreateAvailabilityCommandHandler : IRequestHandler<CreateAvailabilityCommand, AvailabilityDto>
{
    private readonly IAgentAvailabilityRepository _repository;
    private readonly IUserRepository _userRepository;

    public CreateAvailabilityCommandHandler(
        IAgentAvailabilityRepository repository,
        IUserRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    public async Task<AvailabilityDto> Handle(CreateAvailabilityCommand request, CancellationToken cancellationToken)
    {
        await EnsureAgentExistsAsync(request.AgentId, cancellationToken);
        await EnsureNoOverlapAsync(request.AgentId, request.WeekDay, request.StartTime, request.EndTime, null, cancellationToken);

        AgentAvailability availability = new(request.AgentId, request.WeekDay, request.StartTime, request.EndTime);

        await _repository.AddAsync(availability, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToDto(availability);
    }

    private async Task EnsureAgentExistsAsync(Guid agentId, CancellationToken cancellationToken)
    {
        User? agent = await _userRepository.GetByIdAsync(agentId, cancellationToken);
        if (agent is null || agent.Role != UserRole.Agent)
            throw new KeyNotFoundException($"Agent '{agentId}' not found.");
    }

    private async Task EnsureNoOverlapAsync(
        Guid agentId, WeekDay weekDay, TimeOnly start, TimeOnly end,
        Guid? excludeId, CancellationToken cancellationToken)
    {
        IReadOnlyList<AgentAvailability> existing =
            await _repository.GetByAgentAndWeekDayAsync(agentId, weekDay, cancellationToken);

        bool hasOverlap = existing.Any(a =>
            a.IsActive && a.Id != excludeId && start < a.EndTime && end > a.StartTime);

        if (hasOverlap)
            throw new InvalidOperationException("Availability interval overlaps with an existing slot.");
    }

    private static AvailabilityDto ToDto(AgentAvailability a) =>
        new(a.Id, a.AgentId, a.WeekDay, a.StartTime, a.EndTime, a.IsActive);
}

public sealed class CreateAvailabilityCommandValidator : AbstractValidator<CreateAvailabilityCommand>
{
    public CreateAvailabilityCommandValidator()
    {
        RuleFor(c => c.AgentId).NotEmpty();
        RuleFor(c => c.WeekDay).IsInEnum();
        RuleFor(c => c.EndTime).GreaterThan(c => c.StartTime)
            .WithMessage("EndTime must be after StartTime.");
    }
}

// ── UpdateAvailability ──────────────────────────────────────────────────────

public sealed record UpdateAvailabilityCommand(Guid Id, TimeOnly StartTime, TimeOnly EndTime)
    : IRequest<AvailabilityDto>;

public sealed class UpdateAvailabilityCommandHandler : IRequestHandler<UpdateAvailabilityCommand, AvailabilityDto>
{
    private readonly IAgentAvailabilityRepository _repository;

    public UpdateAvailabilityCommandHandler(IAgentAvailabilityRepository repository)
    {
        _repository = repository;
    }

    public async Task<AvailabilityDto> Handle(UpdateAvailabilityCommand request, CancellationToken cancellationToken)
    {
        AgentAvailability availability = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Availability '{request.Id}' not found.");

        await EnsureNoOverlapAsync(availability, request.StartTime, request.EndTime, cancellationToken);

        availability.Update(request.StartTime, request.EndTime);
        await _repository.SaveChangesAsync(cancellationToken);

        return new AvailabilityDto(availability.Id, availability.AgentId, availability.WeekDay,
            availability.StartTime, availability.EndTime, availability.IsActive);
    }

    private async Task EnsureNoOverlapAsync(
        AgentAvailability current, TimeOnly start, TimeOnly end, CancellationToken cancellationToken)
    {
        IReadOnlyList<AgentAvailability> existing =
            await _repository.GetByAgentAndWeekDayAsync(current.AgentId, current.WeekDay, cancellationToken);

        bool hasOverlap = existing.Any(a =>
            a.IsActive && a.Id != current.Id && start < a.EndTime && end > a.StartTime);

        if (hasOverlap)
            throw new InvalidOperationException("Availability interval overlaps with an existing slot.");
    }
}

public sealed class UpdateAvailabilityCommandValidator : AbstractValidator<UpdateAvailabilityCommand>
{
    public UpdateAvailabilityCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.EndTime).GreaterThan(c => c.StartTime)
            .WithMessage("EndTime must be after StartTime.");
    }
}

// ── DeleteAvailability ──────────────────────────────────────────────────────

public sealed record DeleteAvailabilityCommand(Guid Id) : IRequest;

public sealed class DeleteAvailabilityCommandHandler : IRequestHandler<DeleteAvailabilityCommand>
{
    private readonly IAgentAvailabilityRepository _repository;

    public DeleteAvailabilityCommandHandler(IAgentAvailabilityRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(DeleteAvailabilityCommand request, CancellationToken cancellationToken)
    {
        AgentAvailability availability = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Availability '{request.Id}' not found.");

        availability.Deactivate();
        await _repository.SaveChangesAsync(cancellationToken);
    }
}

public sealed class DeleteAvailabilityCommandValidator : AbstractValidator<DeleteAvailabilityCommand>
{
    public DeleteAvailabilityCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}
