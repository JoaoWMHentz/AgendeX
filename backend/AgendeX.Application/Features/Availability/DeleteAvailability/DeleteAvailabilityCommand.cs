using AgendeX.Domain.Entities;
using AgendeX.Domain.Interfaces;
using MediatR;

namespace AgendeX.Application.Features.Availability;

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
