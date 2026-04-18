using AgendeX.Application.Features.Appointments;
using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace AgendeX.Tests.Application.Appointments;

public sealed class RejectAppointmentCommandHandlerTests
{
    [Fact]
    public async Task Handle_PendingAppointmentAndCorrectAgent_RejectsAndReturnsDto()
    {
        User agent = new("Bob", "bob@email.com", "hash", UserRole.Agent);
        User client = new("Alice", "alice@email.com", "hash", UserRole.Client);
        
        Appointment appointment = new("Service", null, 1, client.Id, agent.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), new TimeOnly(14, 0), null);

        Mock<IAppointmentRepository> repositoryMock = new();

        repositoryMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        RejectAppointmentCommandHandler handler = new(repositoryMock.Object);

        AppointmentDto result = await handler.Handle(
            new RejectAppointmentCommand(appointment.Id, agent.Id, "Client not available"),
            CancellationToken.None);

        appointment.Status.Should().Be(AppointmentStatus.Rejected);
        appointment.RejectionReason.Should().Be("Client not available");
        result.Status.Should().Be(AppointmentStatus.Rejected);

        repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AppointmentNotFound_ThrowsKeyNotFoundException()
    {
        Mock<IAppointmentRepository> repositoryMock = new();

        repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        RejectAppointmentCommandHandler handler = new(repositoryMock.Object);

        Func<Task> action = async () => await handler.Handle(
            new RejectAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), "Reason"),
            CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WrongAgent_ThrowsUnauthorizedAccessException()
    {
        User agent = new("Bob", "bob@email.com", "hash", UserRole.Agent);
        User wrongAgent = new("Charlie", "charlie@email.com", "hash", UserRole.Agent);
        User client = new("Alice", "alice@email.com", "hash", UserRole.Client);
        
        Appointment appointment = new("Service", null, 1, client.Id, agent.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), new TimeOnly(14, 0), null);

        Mock<IAppointmentRepository> repositoryMock = new();

        repositoryMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        RejectAppointmentCommandHandler handler = new(repositoryMock.Object);

        Func<Task> action = async () => await handler.Handle(
            new RejectAppointmentCommand(appointment.Id, wrongAgent.Id, "Reason"),
            CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_NotPendingStatus_ThrowsInvalidOperationException()
    {
        User agent = new("Bob", "bob@email.com", "hash", UserRole.Agent);
        User client = new("Alice", "alice@email.com", "hash", UserRole.Client);
        
        Appointment appointment = new("Service", null, 1, client.Id, agent.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), new TimeOnly(14, 0), null);
        appointment.Confirm(); // Already confirmed

        Mock<IAppointmentRepository> repositoryMock = new();

        repositoryMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        RejectAppointmentCommandHandler handler = new(repositoryMock.Object);

        Func<Task> action = async () => await handler.Handle(
            new RejectAppointmentCommand(appointment.Id, agent.Id, "Reason"),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Only pending appointments can be rejected*");
    }
}
