using AgendeX.Domain.Entities;
using AgendeX.Domain.Interfaces;
using MediatR;

namespace AgendeX.Application.Features.ServiceTypes;

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
