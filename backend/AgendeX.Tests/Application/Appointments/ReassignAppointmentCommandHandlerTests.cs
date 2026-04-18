using AgendeX.Application.Features.Appointments;
using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace AgendeX.Tests.Application.Appointments;

public sealed class ReassignAppointmentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidReassignmentToPendingAppointment_Succeeds()
    {
        User oldAgent = new("Bob", "bob@email.com", "hash", UserRole.Agent);
        User newAgent = new("Charlie", "charlie@email.com", "hash", UserRole.Agent);
        User client = new("Alice", "alice@email.com", "hash", UserRole.Client);
        
        Appointment appointment = new("Service", null, 1, client.Id, oldAgent.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), new TimeOnly(14, 0), null);

        Mock<IAppointmentRepository> appointmentRepositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();

        appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(newAgent.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newAgent);

        appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        ReassignAppointmentCommandHandler handler = new(
            appointmentRepositoryMock.Object,
            userRepositoryMock.Object);

        AppointmentDto result = await handler.Handle(
            new ReassignAppointmentCommand(appointment.Id, newAgent.Id),
            CancellationToken.None);

        appointment.AgentId.Should().Be(newAgent.Id);

        appointmentRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AppointmentNotFound_ThrowsKeyNotFoundException()
    {
        Mock<IAppointmentRepository> appointmentRepositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();

        appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        ReassignAppointmentCommandHandler handler = new(
            appointmentRepositoryMock.Object,
            userRepositoryMock.Object);

        Func<Task> action = async () => await handler.Handle(
            new ReassignAppointmentCommand(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_NewAgentNotFound_ThrowsKeyNotFoundException()
    {
        User oldAgent = new("Bob", "bob@email.com", "hash", UserRole.Agent);
        User client = new("Alice", "alice@email.com", "hash", UserRole.Client);
        
        Appointment appointment = new("Service", null, 1, client.Id, oldAgent.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), new TimeOnly(14, 0), null);

        Mock<IAppointmentRepository> appointmentRepositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();

        appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        ReassignAppointmentCommandHandler handler = new(
            appointmentRepositoryMock.Object,
            userRepositoryMock.Object);

        Func<Task> action = async () => await handler.Handle(
            new ReassignAppointmentCommand(appointment.Id, Guid.NewGuid()),
            CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_NewAgentIsNotAgent_ThrowsKeyNotFoundException()
    {
        User oldAgent = new("Bob", "bob@email.com", "hash", UserRole.Agent);
        User notAnAgent = new("Charlie", "charlie@email.com", "hash", UserRole.Client);
        User client = new("Alice", "alice@email.com", "hash", UserRole.Client);
        
        Appointment appointment = new("Service", null, 1, client.Id, oldAgent.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), new TimeOnly(14, 0), null);

        Mock<IAppointmentRepository> appointmentRepositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();

        appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(notAnAgent.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notAnAgent);

        ReassignAppointmentCommandHandler handler = new(
            appointmentRepositoryMock.Object,
            userRepositoryMock.Object);

        Func<Task> action = async () => await handler.Handle(
            new ReassignAppointmentCommand(appointment.Id, notAnAgent.Id),
            CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_CompletedAppointment_ThrowsInvalidOperationException()
    {
        User oldAgent = new("Bob", "bob@email.com", "hash", UserRole.Agent);
        User newAgent = new("Charlie", "charlie@email.com", "hash", UserRole.Agent);
        User client = new("Alice", "alice@email.com", "hash", UserRole.Client);
        
        Appointment appointment = new("Service", null, 1, client.Id, oldAgent.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), new TimeOnly(14, 0), null);
        appointment.Confirm();
        appointment.Complete(null);

        Mock<IAppointmentRepository> appointmentRepositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();

        appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(newAgent.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newAgent);

        ReassignAppointmentCommandHandler handler = new(
            appointmentRepositoryMock.Object,
            userRepositoryMock.Object);

        Func<Task> action = async () => await handler.Handle(
            new ReassignAppointmentCommand(appointment.Id, newAgent.Id),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>();
    }
}
