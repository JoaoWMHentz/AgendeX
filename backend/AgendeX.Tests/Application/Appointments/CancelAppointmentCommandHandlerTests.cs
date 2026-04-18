using AgendeX.Application.Features.Appointments;
using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace AgendeX.Tests.Application.Appointments;

public sealed class CancelAppointmentCommandHandlerTests
{
    [Fact]
    public async Task Handle_AdminCancelAnyAppointment_Succeeds()
    {
        User agent = new("Bob", "bob@email.com", "hash", UserRole.Agent);
        User client = new("Alice", "alice@email.com", "hash", UserRole.Client);
        
        Appointment appointment = new("Service", null, 1, client.Id, agent.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), new TimeOnly(14, 0), null);
        appointment.Confirm();

        Mock<IAppointmentRepository> repositoryMock = new();

        repositoryMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        CancelAppointmentCommandHandler handler = new(repositoryMock.Object);

        AppointmentDto result = await handler.Handle(
            new CancelAppointmentCommand(appointment.Id, Guid.NewGuid(), isAdmin: true),
            CancellationToken.None);

        appointment.Status.Should().Be(AppointmentStatus.Canceled);
        appointment.CanceledAt.Should().NotBeNull();

        repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ClientCancelOwnPendingAppointment_Succeeds()
    {
        User client = new("Alice", "alice@email.com", "hash", UserRole.Client);
        User agent = new("Bob", "bob@email.com", "hash", UserRole.Agent);
        
        Appointment appointment = new("Service", null, 1, client.Id, agent.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), new TimeOnly(14, 0), null);

        Mock<IAppointmentRepository> repositoryMock = new();

        repositoryMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        CancelAppointmentCommandHandler handler = new(repositoryMock.Object);

        AppointmentDto result = await handler.Handle(
            new CancelAppointmentCommand(appointment.Id, client.Id, isAdmin: false),
            CancellationToken.None);

        appointment.Status.Should().Be(AppointmentStatus.Canceled);
    }

    [Fact]
    public async Task Handle_ClientCancelOtherClientAppointment_ThrowsUnauthorizedAccessException()
    {
        User client1 = new("Alice", "alice@email.com", "hash", UserRole.Client);
        User client2 = new("Bob", "bob@email.com", "hash", UserRole.Client);
        User agent = new("Charlie", "charlie@email.com", "hash", UserRole.Agent);
        
        Appointment appointment = new("Service", null, 1, client1.Id, agent.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), new TimeOnly(14, 0), null);

        Mock<IAppointmentRepository> repositoryMock = new();

        repositoryMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        CancelAppointmentCommandHandler handler = new(repositoryMock.Object);

        Func<Task> action = async () => await handler.Handle(
            new CancelAppointmentCommand(appointment.Id, client2.Id, isAdmin: false),
            CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_ClientCancelAlreadyOccurredAppointment_ThrowsInvalidOperationException()
    {
        User client = new("Alice", "alice@email.com", "hash", UserRole.Client);
        User agent = new("Bob", "bob@email.com", "hash", UserRole.Agent);
        
        // Appointment in the past
        Appointment appointment = new("Service", null, 1, client.Id, agent.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), new TimeOnly(14, 0), null);
        appointment.Confirm();

        Mock<IAppointmentRepository> repositoryMock = new();

        repositoryMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        CancelAppointmentCommandHandler handler = new(repositoryMock.Object);

        Func<Task> action = async () => await handler.Handle(
            new CancelAppointmentCommand(appointment.Id, client.Id, isAdmin: false),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_AdminCancelCompletedAppointment_ThrowsInvalidOperationException()
    {
        User client = new("Alice", "alice@email.com", "hash", UserRole.Client);
        User agent = new("Bob", "bob@email.com", "hash", UserRole.Agent);
        
        Appointment appointment = new("Service", null, 1, client.Id, agent.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), new TimeOnly(14, 0), null);
        appointment.Confirm();
        appointment.Complete(null);

        Mock<IAppointmentRepository> repositoryMock = new();

        repositoryMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        CancelAppointmentCommandHandler handler = new(repositoryMock.Object);

        Func<Task> action = async () => await handler.Handle(
            new CancelAppointmentCommand(appointment.Id, Guid.NewGuid(), isAdmin: true),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>();
    }
}
