using AgendeX.Domain.Entities;
using AgendeX.Domain.Interfaces;
using MediatR;

namespace AgendeX.Application.Features.Appointments;

public sealed class GetAppointmentsQueryHandler
    : IRequestHandler<GetAppointmentsQuery, IReadOnlyList<AppointmentDto>>
{
    private readonly IAppointmentRepository _repository;

    public GetAppointmentsQueryHandler(IAppointmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AppointmentDto>> Handle(
        GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<Appointment> appointments = await _repository.GetAllAsync(
            request.ClientId, request.AgentId, request.ServiceTypeId,
            request.Status, request.From, request.To, cancellationToken);

        return appointments.Select(AppointmentMapper.ToDto).ToList().AsReadOnly();
    }
}
