using AgendeX.Application.Features.Appointments;
using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace AgendeX.Tests.Application.Appointments;

public sealed class CreateAppointmentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidData_CreatesAppointmentAndReturnsDto()
    {
        User client = new("Alice", "alice@email.com", "hash", UserRole.Client);
        User agent = new("Bob", "bob@email.com", "hash", UserRole.Agent);
        ServiceType serviceType = new(1, "Consulting");

        DateOnly appointmentDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));
        TimeOnly appointmentTime = new(14, 0);

        AgentAvailability availability = new(agent.Id, (WeekDay)appointmentDate.DayOfWeek, new TimeOnly(13, 0), new TimeOnly(18, 0));

        Appointment? persistedAppointment = null;

        Mock<IAppointmentRepository> appointmentRepositoryMock = new();
        Mock<IAgentAvailabilityRepository> availabilityRepositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IServiceTypeRepository> serviceTypeRepositoryMock = new();

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(agent.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agent);

        serviceTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(serviceType.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceType);

        availabilityRepositoryMock
            .Setup(r => r.GetByAgentAndWeekDayAsync(agent.Id, (WeekDay)appointmentDate.DayOfWeek, It.IsAny<CancellationToken>()))
            .ReturnsAsync([availability]);

        appointmentRepositoryMock
            .Setup(r => r.HasConflictAsync(agent.Id, appointmentDate, appointmentTime, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        appointmentRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
            .Callback<Appointment, CancellationToken>((appt, _) => persistedAppointment = appt)
            .Returns(Task.CompletedTask);

        appointmentRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => persistedAppointment);

        CreateAppointmentCommandHandler handler = new(
            appointmentRepositoryMock.Object,
            availabilityRepositoryMock.Object,
            userRepositoryMock.Object,
            serviceTypeRepositoryMock.Object);

        CreateAppointmentCommand command = new("Service Title", "Description", serviceType.Id, client.Id, agent.Id, appointmentDate, appointmentTime, "Notes");

        AppointmentDto result = await handler.Handle(command, CancellationToken.None);

        result.Title.Should().Be("Service Title");
        result.Status.Should().Be(AppointmentStatus.PendingConfirmation);
        persistedAppointment.Should().NotBeNull();

        appointmentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Once);
        appointmentRepositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AgentNotFound_ThrowsKeyNotFoundException()
    {
        Mock<IAppointmentRepository> appointmentRepositoryMock = new();
        Mock<IAgentAvailabilityRepository> availabilityRepositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IServiceTypeRepository> serviceTypeRepositoryMock = new();

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        CreateAppointmentCommandHandler handler = new(
            appointmentRepositoryMock.Object,
            availabilityRepositoryMock.Object,
            userRepositoryMock.Object,
            serviceTypeRepositoryMock.Object);

        Func<Task> action = async () => await handler.Handle(
            new CreateAppointmentCommand("Title", null, 1, Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), new TimeOnly(14, 0), null),
            CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_AgentIsNotAgentRole_ThrowsKeyNotFoundException()
    {
        User notAnAgent = new("Charlie", "charlie@email.com", "hash", UserRole.Client);

        Mock<IAppointmentRepository> appointmentRepositoryMock = new();
        Mock<IAgentAvailabilityRepository> availabilityRepositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IServiceTypeRepository> serviceTypeRepositoryMock = new();

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(notAnAgent.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notAnAgent);

        CreateAppointmentCommandHandler handler = new(
            appointmentRepositoryMock.Object,
            availabilityRepositoryMock.Object,
            userRepositoryMock.Object,
            serviceTypeRepositoryMock.Object);

        Func<Task> action = async () => await handler.Handle(
            new CreateAppointmentCommand("Title", null, 1, Guid.NewGuid(), notAnAgent.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), new TimeOnly(14, 0), null),
            CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_ServiceTypeNotFound_ThrowsKeyNotFoundException()
    {
        User agent = new("Bob", "bob@email.com", "hash", UserRole.Agent);

        Mock<IAppointmentRepository> appointmentRepositoryMock = new();
        Mock<IAgentAvailabilityRepository> availabilityRepositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IServiceTypeRepository> serviceTypeRepositoryMock = new();

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(agent.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agent);

        serviceTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceType?)null);

        CreateAppointmentCommandHandler handler = new(
            appointmentRepositoryMock.Object,
            availabilityRepositoryMock.Object,
            userRepositoryMock.Object,
            serviceTypeRepositoryMock.Object);

        Func<Task> action = async () => await handler.Handle(
            new CreateAppointmentCommand("Title", null, 999, Guid.NewGuid(), agent.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), new TimeOnly(14, 0), null),
            CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_TimeNotWithinAvailability_ThrowsInvalidOperationException()
    {
        User agent = new("Bob", "bob@email.com", "hash", UserRole.Agent);
        ServiceType serviceType = new(1, "Consulting");

        DateOnly appointmentDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));
        TimeOnly appointmentTime = new(20, 0); // After availability window

        AgentAvailability availability = new(agent.Id, (WeekDay)appointmentDate.DayOfWeek, new TimeOnly(13, 0), new TimeOnly(18, 0));

        Mock<IAppointmentRepository> appointmentRepositoryMock = new();
        Mock<IAgentAvailabilityRepository> availabilityRepositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IServiceTypeRepository> serviceTypeRepositoryMock = new();

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(agent.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agent);

        serviceTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(serviceType.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceType);

        availabilityRepositoryMock
            .Setup(r => r.GetByAgentAndWeekDayAsync(agent.Id, (WeekDay)appointmentDate.DayOfWeek, It.IsAny<CancellationToken>()))
            .ReturnsAsync([availability]);

        CreateAppointmentCommandHandler handler = new(
            appointmentRepositoryMock.Object,
            availabilityRepositoryMock.Object,
            userRepositoryMock.Object,
            serviceTypeRepositoryMock.Object);

        Func<Task> action = async () => await handler.Handle(
            new CreateAppointmentCommand("Title", null, serviceType.Id, Guid.NewGuid(), agent.Id, appointmentDate, appointmentTime, null),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not within any active availability window*");
    }

    [Fact]
    public async Task Handle_ConflictWithExistingAppointment_ThrowsInvalidOperationException()
    {
        User agent = new("Bob", "bob@email.com", "hash", UserRole.Agent);
        ServiceType serviceType = new(1, "Consulting");

        DateOnly appointmentDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));
        TimeOnly appointmentTime = new(14, 0);

        AgentAvailability availability = new(agent.Id, (WeekDay)appointmentDate.DayOfWeek, new TimeOnly(13, 0), new TimeOnly(18, 0));

        Mock<IAppointmentRepository> appointmentRepositoryMock = new();
        Mock<IAgentAvailabilityRepository> availabilityRepositoryMock = new();
        Mock<IUserRepository> userRepositoryMock = new();
        Mock<IServiceTypeRepository> serviceTypeRepositoryMock = new();

        userRepositoryMock
            .Setup(r => r.GetByIdAsync(agent.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agent);

        serviceTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(serviceType.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serviceType);

        availabilityRepositoryMock
            .Setup(r => r.GetByAgentAndWeekDayAsync(agent.Id, (WeekDay)appointmentDate.DayOfWeek, It.IsAny<CancellationToken>()))
            .ReturnsAsync([availability]);

        appointmentRepositoryMock
            .Setup(r => r.HasConflictAsync(agent.Id, appointmentDate, appointmentTime, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        CreateAppointmentCommandHandler handler = new(
            appointmentRepositoryMock.Object,
            availabilityRepositoryMock.Object,
            userRepositoryMock.Object,
            serviceTypeRepositoryMock.Object);

        Func<Task> action = async () => await handler.Handle(
            new CreateAppointmentCommand("Title", null, serviceType.Id, Guid.NewGuid(), agent.Id, appointmentDate, appointmentTime, null),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*agent already has an appointment*");
    }
}
