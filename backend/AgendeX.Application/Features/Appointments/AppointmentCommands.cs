using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using FluentValidation;
using MediatR;

namespace AgendeX.Application.Features.Appointments;

// ── CreateAppointment ───────────────────────────────────────────────────────

public sealed record CreateAppointmentCommand(
    string Title,
    string? Description,
    int ServiceTypeId,
    Guid ClientId,
    Guid AgentId,
    DateOnly Date,
    TimeOnly Time,
    string? Notes
) : IRequest<AppointmentDto>;

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

public sealed class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(c => c.Title).NotEmpty().MaximumLength(200);
        RuleFor(c => c.ServiceTypeId).GreaterThan(0);
        RuleFor(c => c.ClientId).NotEmpty();
        RuleFor(c => c.AgentId).NotEmpty();
        RuleFor(c => c.Date)
            .Must(d => d >= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date cannot be in the past.");
    }
}

// ── ConfirmAppointment ──────────────────────────────────────────────────────

public sealed record ConfirmAppointmentCommand(Guid Id, Guid AgentId) : IRequest<AppointmentDto>;

public sealed class ConfirmAppointmentCommandHandler : IRequestHandler<ConfirmAppointmentCommand, AppointmentDto>
{
    private readonly IAppointmentRepository _repository;

    public ConfirmAppointmentCommandHandler(IAppointmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<AppointmentDto> Handle(ConfirmAppointmentCommand request, CancellationToken cancellationToken)
    {
        Appointment appointment = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Appointment '{request.Id}' not found.");

        if (appointment.AgentId != request.AgentId)
            throw new UnauthorizedAccessException("You are not the assigned agent for this appointment.");

        if (appointment.Status != AppointmentStatus.PendingConfirmation)
            throw new InvalidOperationException("Only pending appointments can be confirmed.");

        appointment.Confirm();
        await _repository.SaveChangesAsync(cancellationToken);

        return AppointmentMapper.ToDto(appointment);
    }
}

// ── RejectAppointment ───────────────────────────────────────────────────────

public sealed record RejectAppointmentCommand(Guid Id, Guid AgentId, string RejectionReason)
    : IRequest<AppointmentDto>;

public sealed class RejectAppointmentCommandHandler : IRequestHandler<RejectAppointmentCommand, AppointmentDto>
{
    private readonly IAppointmentRepository _repository;

    public RejectAppointmentCommandHandler(IAppointmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<AppointmentDto> Handle(RejectAppointmentCommand request, CancellationToken cancellationToken)
    {
        Appointment appointment = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Appointment '{request.Id}' not found.");

        if (appointment.AgentId != request.AgentId)
            throw new UnauthorizedAccessException("You are not the assigned agent for this appointment.");

        if (appointment.Status != AppointmentStatus.PendingConfirmation)
            throw new InvalidOperationException("Only pending appointments can be rejected.");

        appointment.Reject(request.RejectionReason);
        await _repository.SaveChangesAsync(cancellationToken);

        return AppointmentMapper.ToDto(appointment);
    }
}

public sealed class RejectAppointmentCommandValidator : AbstractValidator<RejectAppointmentCommand>
{
    public RejectAppointmentCommandValidator()
    {
        RuleFor(c => c.RejectionReason).NotEmpty().MaximumLength(500);
    }
}

// ── CancelAppointment ───────────────────────────────────────────────────────

public sealed record CancelAppointmentCommand(Guid Id, Guid RequestingUserId, bool IsAdmin)
    : IRequest<AppointmentDto>;

public sealed class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand, AppointmentDto>
{
    private readonly IAppointmentRepository _repository;

    public CancelAppointmentCommandHandler(IAppointmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<AppointmentDto> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        Appointment appointment = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Appointment '{request.Id}' not found.");

        if (request.IsAdmin)
            ValidateAdminCancel(appointment);
        else
            ValidateClientCancel(appointment, request.RequestingUserId);

        appointment.Cancel();
        await _repository.SaveChangesAsync(cancellationToken);

        return AppointmentMapper.ToDto(appointment);
    }

    private static void ValidateAdminCancel(Appointment appointment)
    {
        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Canceled)
            throw new InvalidOperationException("Appointment is already completed or canceled.");
    }

    private static void ValidateClientCancel(Appointment appointment, Guid clientId)
    {
        if (appointment.ClientId != clientId)
            throw new UnauthorizedAccessException("You can only cancel your own appointments.");

        if (appointment.Status is not (AppointmentStatus.PendingConfirmation or AppointmentStatus.Confirmed))
            throw new InvalidOperationException("Appointment cannot be canceled at its current status.");

        if (appointment.Date.ToDateTime(appointment.Time) <= DateTime.UtcNow)
            throw new InvalidOperationException("Cannot cancel an appointment that has already occurred.");
    }
}

// ── CompleteAppointment ─────────────────────────────────────────────────────

public sealed record CompleteAppointmentCommand(Guid Id, Guid AgentId, string? ServiceSummary)
    : IRequest<AppointmentDto>;

public sealed class CompleteAppointmentCommandHandler : IRequestHandler<CompleteAppointmentCommand, AppointmentDto>
{
    private readonly IAppointmentRepository _repository;

    public CompleteAppointmentCommandHandler(IAppointmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<AppointmentDto> Handle(CompleteAppointmentCommand request, CancellationToken cancellationToken)
    {
        Appointment appointment = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Appointment '{request.Id}' not found.");

        if (appointment.AgentId != request.AgentId)
            throw new UnauthorizedAccessException("You are not the assigned agent for this appointment.");

        if (appointment.Status != AppointmentStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed appointments can be marked as completed.");

        if (appointment.Date.ToDateTime(appointment.Time) > DateTime.UtcNow)
            throw new InvalidOperationException("Cannot complete an appointment that has not occurred yet.");

        appointment.Complete(request.ServiceSummary);
        await _repository.SaveChangesAsync(cancellationToken);

        return AppointmentMapper.ToDto(appointment);
    }
}

// ── ReassignAppointment ─────────────────────────────────────────────────────

public sealed record ReassignAppointmentCommand(Guid Id, Guid NewAgentId) : IRequest<AppointmentDto>;

public sealed class ReassignAppointmentCommandHandler : IRequestHandler<ReassignAppointmentCommand, AppointmentDto>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUserRepository _userRepository;

    public ReassignAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IUserRepository userRepository)
    {
        _appointmentRepository = appointmentRepository;
        _userRepository = userRepository;
    }

    public async Task<AppointmentDto> Handle(ReassignAppointmentCommand request, CancellationToken cancellationToken)
    {
        Appointment appointment = await _appointmentRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Appointment '{request.Id}' not found.");

        User? newAgent = await _userRepository.GetByIdAsync(request.NewAgentId, cancellationToken);
        if (newAgent is null || newAgent.Role != UserRole.Agent)
            throw new KeyNotFoundException($"Agent '{request.NewAgentId}' not found.");

        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Canceled)
            throw new InvalidOperationException("Cannot reassign a completed or canceled appointment.");

        appointment.Reassign(request.NewAgentId);
        await _appointmentRepository.SaveChangesAsync(cancellationToken);

        Appointment updated = await _appointmentRepository.GetByIdAsync(appointment.Id, cancellationToken) ?? appointment;
        return AppointmentMapper.ToDto(updated);
    }
}

public sealed class ReassignAppointmentCommandValidator : AbstractValidator<ReassignAppointmentCommand>
{
    public ReassignAppointmentCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.NewAgentId).NotEmpty();
    }
}
