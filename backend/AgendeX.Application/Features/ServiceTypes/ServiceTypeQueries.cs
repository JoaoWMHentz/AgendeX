using AgendeX.Domain.Entities;
using AgendeX.Domain.Interfaces;
using MediatR;

namespace AgendeX.Application.Features.ServiceTypes;

// ── GetServiceTypes ─────────────────────────────────────────────────────────

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

// ── GetServiceTypeById ──────────────────────────────────────────────────────

public sealed record GetServiceTypeByIdQuery(int Id) : IRequest<ServiceTypeDto>;

public sealed class GetServiceTypeByIdQueryHandler : IRequestHandler<GetServiceTypeByIdQuery, ServiceTypeDto>
{
    private readonly IServiceTypeRepository _repository;

    public GetServiceTypeByIdQueryHandler(IServiceTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceTypeDto> Handle(GetServiceTypeByIdQuery request, CancellationToken cancellationToken)
    {
        ServiceType type = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"ServiceType '{request.Id}' not found.");

        return new ServiceTypeDto(type.Id, type.Description);
    }
}
