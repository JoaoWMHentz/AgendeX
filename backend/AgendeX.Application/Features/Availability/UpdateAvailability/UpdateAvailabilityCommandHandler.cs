using AgendeX.Domain.Entities;
using AgendeX.Domain.Interfaces;
using MediatR;

namespace AgendeX.Application.Features.Availability;

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
