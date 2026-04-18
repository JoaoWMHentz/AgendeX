using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using MediatR;

namespace AgendeX.Application.Features.Appointments;

public sealed class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, AppointmentDto>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAgentAvailabilityRepository _availabilityRepository;
    private readonly IUserRepository _userRepository;
    private readonly IServiceTypeRepository _serviceTypeRepository;

    public CreateAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IAgentAvailabilityRepository availabilityRepository,
        IUserRepository userRepository,
        IServiceTypeRepository serviceTypeRepository)
    {
        _appointmentRepository = appointmentRepository;
        _availabilityRepository = availabilityRepository;
        _userRepository = userRepository;
        _serviceTypeRepository = serviceTypeRepository;
    }

    public async Task<AppointmentDto> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        await EnsureAgentExistsAsync(request.AgentId, cancellationToken);
        await EnsureServiceTypeExistsAsync(request.ServiceTypeId, cancellationToken);
        await EnsureTimeIsWithinAvailabilityAsync(request.AgentId, request.Date, request.Time, cancellationToken);
        await EnsureNoConflictAsync(request.AgentId, request.Date, request.Time, cancellationToken);

        Appointment appointment = new(
            request.Title, request.Description, request.ServiceTypeId,
            request.ClientId, request.AgentId, request.Date, request.Time, request.Notes);

        await _appointmentRepository.AddAsync(appointment, cancellationToken);
        await _appointmentRepository.SaveChangesAsync(cancellationToken);

        Appointment created = await _appointmentRepository.GetByIdAsync(appointment.Id, cancellationToken) ?? appointment;
        return AppointmentMapper.ToDto(created);
    }

    private async Task EnsureAgentExistsAsync(Guid agentId, CancellationToken cancellationToken)
    {
        User? agent = await _userRepository.GetByIdAsync(agentId, cancellationToken);
        if (agent is null || agent.Role != UserRole.Agent)
            throw new KeyNotFoundException($"Agent '{agentId}' not found.");
    }

    private async Task EnsureServiceTypeExistsAsync(int serviceTypeId, CancellationToken cancellationToken)
    {
        ServiceType? serviceType = await _serviceTypeRepository.GetByIdAsync(serviceTypeId, cancellationToken);
        if (serviceType is null)
            throw new KeyNotFoundException($"ServiceType '{serviceTypeId}' not found.");
    }

    private async Task EnsureTimeIsWithinAvailabilityAsync(
        Guid agentId, DateOnly date, TimeOnly time, CancellationToken cancellationToken)
    {
        WeekDay weekDay = (WeekDay)date.DayOfWeek;
        IReadOnlyList<AgentAvailability> slots =
            await _availabilityRepository.GetByAgentAndWeekDayAsync(agentId, weekDay, cancellationToken);

        bool withinSlot = slots.Any(s => s.IsActive && time >= s.StartTime && time < s.EndTime);
        if (!withinSlot)
            throw new InvalidOperationException("The selected time is not within any active availability window for this agent.");
    }

    private async Task EnsureNoConflictAsync(
        Guid agentId, DateOnly date, TimeOnly time, CancellationToken cancellationToken)
    {
        bool hasConflict = await _appointmentRepository.HasConflictAsync(agentId, date, time, null, cancellationToken);
        if (hasConflict)
            throw new InvalidOperationException("The agent already has an appointment at this time.");
    }
}
