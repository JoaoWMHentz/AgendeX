using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using MediatR;

namespace AgendeX.Application.Features.Availability;

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
