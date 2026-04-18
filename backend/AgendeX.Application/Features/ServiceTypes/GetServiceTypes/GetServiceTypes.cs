using AgendeX.Domain.Entities;
using AgendeX.Domain.Interfaces;
using MediatR;

namespace AgendeX.Application.Features.ServiceTypes;

public sealed record GetServiceTypesQuery : IRequest<IReadOnlyList<ServiceTypeDto>>;

public sealed class GetServiceTypesQueryHandler : IRequestHandler<GetServiceTypesQuery, IReadOnlyList<ServiceTypeDto>>
{
    private readonly IServiceTypeRepository _repository;

    public GetServiceTypesQueryHandler(IServiceTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ServiceTypeDto>> Handle(GetServiceTypesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<ServiceType> types = await _repository.GetAllAsync(cancellationToken);
        return types.Select(t => new ServiceTypeDto(t.Id, t.Description)).ToList().AsReadOnly();
    }
}
