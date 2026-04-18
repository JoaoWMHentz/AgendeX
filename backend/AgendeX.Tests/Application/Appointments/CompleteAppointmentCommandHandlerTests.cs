using AgendeX.Application.Features.Appointments;
using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace AgendeX.Tests.Application.Appointments;

public sealed class CompleteAppointmentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ConfirmedAppointmentInThePast_MarksAsCompleted()
    {
        User agent = new("Bob", "bob@email.com", "hash", UserRole.Agent);
        User client = new("Alice", "alice@email.com", "hash", UserRole.Client);
        
        // Appointment in the past
        Appointment appointment = new("Service", null, 1, client.Id, agent.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), new TimeOnly(14, 0), null);
        appointment.Confirm();

        Mock<IAppointmentRepository> repositoryMock = new();

        repositoryMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        CompleteAppointmentCommandHandler handler = new(repositoryMock.Object);

        AppointmentDto result = await handler.Handle(
            new CompleteAppointmentCommand(appointment.Id, agent.Id, "Service completed successfully"),
            CancellationToken.None);

        appointment.Status.Should().Be(AppointmentStatus.Completed);
        appointment.ServiceSummary.Should().Be("Service completed successfully");

        repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AppointmentNotFound_ThrowsKeyNotFoundException()
    {
        Mock<IAppointmentRepository> repositoryMock = new();

        repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        CompleteAppointmentCommandHandler handler = new(repositoryMock.Object);

        Func<Task> action = async () => await handler.Handle(
            new CompleteAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), null),
            CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WrongAgent_ThrowsUnauthorizedAccessException()
    {
        User agent = new("Bob", "bob@email.com", "hash", UserRole.Agent);
        User wrongAgent = new("Charlie", "charlie@email.com", "hash", UserRole.Agent);
        User client = new("Alice", "alice@email.com", "hash", UserRole.Client);
        
        Appointment appointment = new("Service", null, 1, client.Id, agent.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), new TimeOnly(14, 0), null);
        appointment.Confirm();

        Mock<IAppointmentRepository> repositoryMock = new();

        repositoryMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        CompleteAppointmentCommandHandler handler = new(repositoryMock.Object);

        Func<Task> action = async () => await handler.Handle(
            new CompleteAppointmentCommand(appointment.Id, wrongAgent.Id, null),
            CancellationToken.None);

        await action.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_NotConfirmedStatus_ThrowsInvalidOperationException()
    {
        User agent = new("Bob", "bob@email.com", "hash", UserRole.Agent);
        User client = new("Alice", "alice@email.com", "hash", UserRole.Client);
        
        Appointment appointment = new("Service", null, 1, client.Id, agent.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), new TimeOnly(14, 0), null);
        // Pending, not confirmed

        Mock<IAppointmentRepository> repositoryMock = new();

        repositoryMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        CompleteAppointmentCommandHandler handler = new(repositoryMock.Object);

        Func<Task> action = async () => await handler.Handle(
            new CompleteAppointmentCommand(appointment.Id, agent.Id, null),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Only confirmed appointments can be marked as completed*");
    }

    [Fact]
    public async Task Handle_FutureAppointment_ThrowsInvalidOperationException()
    {
        User agent = new("Bob", "bob@email.com", "hash", UserRole.Agent);
        User client = new("Alice", "alice@email.com", "hash", UserRole.Client);
        
        Appointment appointment = new("Service", null, 1, client.Id, agent.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), new TimeOnly(14, 0), null);
        appointment.Confirm();

        Mock<IAppointmentRepository> repositoryMock = new();

        repositoryMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        CompleteAppointmentCommandHandler handler = new(repositoryMock.Object);

        Func<Task> action = async () => await handler.Handle(
            new CompleteAppointmentCommand(appointment.Id, agent.Id, null),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot complete an appointment that has not occurred yet*");
    }
}
