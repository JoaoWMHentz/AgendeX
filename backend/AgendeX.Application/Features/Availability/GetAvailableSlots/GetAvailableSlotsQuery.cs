using MediatR;

namespace AgendeX.Application.Features.Availability;

public sealed record GetAvailableSlotsQuery(Guid AgentId, DateOnly Date)
    : IRequest<IReadOnlyList<AvailableSlotDto>>;
