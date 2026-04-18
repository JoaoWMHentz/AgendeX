using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using System.Reflection;

namespace AgendeX.Tests.Application.Common;

internal static class EntityTestFactory
{
    public static Appointment CreateAppointment(
        Guid? id = null,
        Guid? clientId = null,
        Guid? agentId = null,
        int serviceTypeId = 1,
        DateOnly? date = null,
        TimeOnly? time = null,
        AppointmentStatus status = AppointmentStatus.PendingConfirmation,
        string? rejectionReason = null,
        string? serviceSummary = null)
    {
        Appointment appointment = new(
            "Appointment Title",
            "Description",
            serviceTypeId,
            clientId ?? Guid.NewGuid(),
            agentId ?? Guid.NewGuid(),
            date ?? DateOnly.FromDateTime(DateTime.UtcNow.Date),
            time ?? new TimeOnly(10, 0),
            "Notes");

        if (id.HasValue)
        {
            SetProperty(appointment, nameof(Appointment.Id), id.Value);
        }

        SetProperty(appointment, nameof(Appointment.Status), status);
        SetProperty(appointment, nameof(Appointment.RejectionReason), rejectionReason);
        SetProperty(appointment, nameof(Appointment.ServiceSummary), serviceSummary);

        PopulateAppointmentNavigations(appointment);
        return appointment;
    }

    public static void PopulateAppointmentNavigations(
        Appointment appointment,
        string serviceTypeDescription = "Consulting",
        string clientName = "Client",
        string agentName = "Agent")
    {
        ServiceType serviceType = new(serviceTypeDescription);
        SetProperty(serviceType, nameof(ServiceType.Id), appointment.ServiceTypeId);

        User client = new(clientName, "client@email.com", "hash", UserRole.Client);
        SetProperty(client, nameof(User.Id), appointment.ClientId);

        User agent = new(agentName, "agent@email.com", "hash", UserRole.Agent);
        SetProperty(agent, nameof(User.Id), appointment.AgentId);

        SetProperty(appointment, nameof(Appointment.ServiceType), serviceType);
        SetProperty(appointment, nameof(Appointment.Client), client);
        SetProperty(appointment, nameof(Appointment.Agent), agent);
    }

    private static void SetProperty<T>(object target, string propertyName, T value)
    {
        PropertyInfo propertyInfo = target.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Property '{propertyName}' not found on type '{target.GetType().Name}'.");

        propertyInfo.SetValue(target, value);
    }
}
