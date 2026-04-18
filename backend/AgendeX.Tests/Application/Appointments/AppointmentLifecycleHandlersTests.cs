using AgendeX.Application.Features.Appointments;
using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using AgendeX.Tests.Application.Common;
using FluentAssertions;
using Moq;

namespace AgendeX.Tests.Application.Appointments;

public sealed class AppointmentLifecycleHandlersTests
{
    [Fact]
    public async Task ConfirmHandle_PendingAppointmentByAssignedAgent_ReturnsConfirmed()
    {
        Mock<IAppointmentRepository> repository = new();
        Guid agentId = Guid.NewGuid();

        Appointment appointment = EntityTestFactory.CreateAppointment(
            agentId: agentId,
            date: DateOnly.FromDateTime(DateTime.UtcNow.Date),
            status: AppointmentStatus.PendingConfirmation);

        repository.Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        ConfirmAppointmentCommandHandler handler = new(repository.Object);

        AppointmentDto result = await handler.Handle(new ConfirmAppointmentCommand(appointment.Id, agentId), CancellationToken.None);

        result.Status.Should().Be(AppointmentStatus.Confirmed);
        repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RejectHandle_AppointmentNotPending_ThrowsInvalidOperationException()
    {
        Mock<IAppointmentRepository> repository = new();
        Guid agentId = Guid.NewGuid();

        Appointment appointment = EntityTestFactory.CreateAppointment(
            agentId: agentId,
            status: AppointmentStatus.Confirmed);

        repository.Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        RejectAppointmentCommandHandler handler = new(repository.Object);

        Func<Task> act = async () => await handler.Handle(
            new RejectAppointmentCommand(appointment.Id, agentId, "Indisponivel"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Only pending appointments can be rejected.");
    }

    [Fact]
    public async Task CancelHandle_ClientCancelsOwnFutureAppointment_ReturnsCanceled()
    {
        Mock<IAppointmentRepository> repository = new();
        Guid clientId = Guid.NewGuid();

        Appointment appointment = EntityTestFactory.CreateAppointment(
            clientId: clientId,
            date: DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)),
            time: new TimeOnly(10, 0),
            status: AppointmentStatus.Confirmed);

        repository.Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        CancelAppointmentCommandHandler handler = new(repository.Object);

        AppointmentDto result = await handler.Handle(
            new CancelAppointmentCommand(appointment.Id, clientId, false),
            CancellationToken.None);

        result.Status.Should().Be(AppointmentStatus.Canceled);
        repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelHandle_ClientCancelsAnotherClientAppointment_ThrowsUnauthorizedAccessException()
    {
        Mock<IAppointmentRepository> repository = new();

        Appointment appointment = EntityTestFactory.CreateAppointment(
            clientId: Guid.NewGuid(),
            date: DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)),
            status: AppointmentStatus.PendingConfirmation);

        repository.Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        CancelAppointmentCommandHandler handler = new(repository.Object);

        Func<Task> act = async () => await handler.Handle(
            new CancelAppointmentCommand(appointment.Id, Guid.NewGuid(), false),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You can only cancel your own appointments.");
    }

    [Fact]
    public async Task CompleteHandle_ConfirmedPastAppointmentByAssignedAgent_ReturnsCompleted()
    {
        Mock<IAppointmentRepository> repository = new();
        Guid agentId = Guid.NewGuid();

        DateTime occurredAt = DateTime.UtcNow.AddHours(-2);
        Appointment appointment = EntityTestFactory.CreateAppointment(
            agentId: agentId,
            date: DateOnly.FromDateTime(occurredAt),
            time: TimeOnly.FromDateTime(occurredAt),
            status: AppointmentStatus.Confirmed);

        repository.Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        CompleteAppointmentCommandHandler handler = new(repository.Object);

        AppointmentDto result = await handler.Handle(
            new CompleteAppointmentCommand(appointment.Id, agentId, "Servico concluido"),
            CancellationToken.None);

        result.Status.Should().Be(AppointmentStatus.Completed);
        result.ServiceSummary.Should().Be("Servico concluido");
    }

    [Fact]
    public async Task CompleteHandle_AppointmentInFuture_ThrowsInvalidOperationException()
    {
        Mock<IAppointmentRepository> repository = new();
        Guid agentId = Guid.NewGuid();

        Appointment appointment = EntityTestFactory.CreateAppointment(
            agentId: agentId,
            date: DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)),
            time: new TimeOnly(10, 0),
            status: AppointmentStatus.Confirmed);

        repository.Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        CompleteAppointmentCommandHandler handler = new(repository.Object);

        Func<Task> act = async () => await handler.Handle(
            new CompleteAppointmentCommand(appointment.Id, agentId, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot complete an appointment that has not occurred yet.");
    }
}
