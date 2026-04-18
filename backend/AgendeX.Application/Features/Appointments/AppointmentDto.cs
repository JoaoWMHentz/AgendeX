using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;

namespace AgendeX.Application.Features.Appointments;

public sealed record AppointmentDto(
    Guid Id,
    string Title,
    string? Description,
    int ServiceTypeId,
    string ServiceTypeDescription,
    Guid ClientId,
    string ClientName,
    Guid AgentId,
    string AgentName,
    DateOnly Date,
    TimeOnly Time,
    AppointmentStatus Status,
    string? RejectionReason,
    string? ServiceSummary,
    DateTime CreatedAt,
    DateTime? ConfirmedAt,
    DateTime? CanceledAt,
    string? Notes);

internal static class AppointmentMapper
{
    internal static AppointmentDto ToDto(Appointment a) => new(
        a.Id,
        a.Title,
        a.Description,
        a.ServiceTypeId,
        a.ServiceType.Description,
        a.ClientId,
        a.Client.Name,
        a.AgentId,
        a.Agent.Name,
        a.Date,
        a.Time,
        a.Status,
        a.RejectionReason,
        a.ServiceSummary,
        a.CreatedAt,
        a.ConfirmedAt,
        a.CanceledAt,
        a.Notes);
}
