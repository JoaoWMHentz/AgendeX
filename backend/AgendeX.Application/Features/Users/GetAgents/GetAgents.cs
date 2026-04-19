using AgendeX.Domain.Entities;
using AgendeX.Domain.Interfaces;
using MediatR;

namespace AgendeX.Application.Features.Users;

public sealed record GetAgentsQuery() : IRequest<IReadOnlyList<AgentLookupDto>>;

public sealed class GetAgentsQueryHandler : IRequestHandler<GetAgentsQuery, IReadOnlyList<AgentLookupDto>>
{
    private readonly IUserRepository _userRepository;

    public GetAgentsQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<AgentLookupDto>> Handle(GetAgentsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<User> agents = await _userRepository.GetActiveAgentsAsync(cancellationToken);
        return agents
            .Select(agent => new AgentLookupDto(agent.Id, agent.Name))
            .ToList()
            .AsReadOnly();
    }
}
