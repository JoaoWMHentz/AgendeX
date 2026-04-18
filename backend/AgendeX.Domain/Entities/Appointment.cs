using AgendeX.Domain.Enums;

namespace AgendeX.Domain.Entities;

public class Appointment
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int ServiceTypeId { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid AgentId { get; private set; }
    public DateOnly Date { get; private set; }
    public TimeOnly Time { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public string? RejectionReason { get; private set; }
    public string? ServiceSummary { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? CanceledAt { get; private set; }
    public string? Notes { get; private set; }

    public ServiceType ServiceType { get; private set; } = null!;
    public User Client { get; private set; } = null!;
    public User Agent { get; private set; } = null!;

    protected Appointment() { }

    public Appointment(
        string title, string? description, int serviceTypeId,
        Guid clientId, Guid agentId, DateOnly date, TimeOnly time, string? notes)
    {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        ServiceTypeId = serviceTypeId;
        ClientId = clientId;
        AgentId = agentId;
        Date = date;
        Time = time;
        Notes = notes;
        Status = AppointmentStatus.PendingConfirmation;
        CreatedAt = DateTime.UtcNow;
    }

    public void Confirm()
    {
        Status = AppointmentStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
    }

    public void Reject(string reason)
    {
        Status = AppointmentStatus.Rejected;
        RejectionReason = reason;
    }

    public void Cancel()
    {
        Status = AppointmentStatus.Canceled;
        CanceledAt = DateTime.UtcNow;
    }

    public void Complete(string? serviceSummary)
    {
        Status = AppointmentStatus.Completed;
        ServiceSummary = serviceSummary;
    }

    public void Reassign(Guid newAgentId) => AgentId = newAgentId;
}
