using AgendeX.Domain.Interfaces;
using MediatR;

namespace AgendeX.Application.Features.Availability;

public sealed class CreateAvailabilityCommandHandler
    : IRequestHandler<CreateAvailabilityCommand, IReadOnlyList<AvailabilityDto>>
{
    private readonly AvailabilityCreationService _creationService;

    public CreateAvailabilityCommandHandler(
        IAgentAvailabilityRepository repository,
        IUserRepository userRepository)
    {
        _creationService = new AvailabilityCreationService(repository, userRepository);
    }

    public Task<IReadOnlyList<AvailabilityDto>> Handle(
        CreateAvailabilityCommand request,
        CancellationToken cancellationToken)
    {
        return _creationService.CreateForWeekDaysAsync(
            request.AgentId,
            request.WeekDays,
            request.StartTime,
            request.EndTime,
            cancellationToken);
    }
}
