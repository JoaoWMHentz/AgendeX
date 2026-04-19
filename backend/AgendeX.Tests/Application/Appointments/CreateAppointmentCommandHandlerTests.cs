using AgendeX.Application.Features.Appointments;
using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using AgendeX.Tests.Application.Common;
using FluentAssertions;
using Moq;

namespace AgendeX.Tests.Application.Appointments;

public sealed class CreateAppointmentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidRequest_ReturnsPendingAppointmentDto()
    {
        Mock<IAppointmentRepository> appointmentRepository = new();
        Mock<IAgentAvailabilityRepository> availabilityRepository = new();
        Mock<IUserRepository> userRepository = new();
        Mock<IServiceTypeRepository> serviceTypeRepository = new();

        Guid agentId = Guid.NewGuid();
        Guid clientId = Guid.NewGuid();
        DateOnly date = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1));
        TimeOnly time = new(9, 0);
        Appointment? persistedAppointment = null;

        CreateAppointmentCommand command = new(
            "Consulta",
            "Detalhes",
            1,
            clientId,
            agentId,
            date,
            time,
            "Observacao");

        userRepository
            .Setup(r => r.GetByIdAsync(agentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User("Agent", "agent@email.com", "hash", UserRole.Agent));

        serviceTypeRepository
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ServiceType("Consulting"));

        availabilityRepository
            .Setup(r => r.GetByAgentAndWeekDayAsync(agentId, (WeekDay)date.DayOfWeek, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AgentAvailability>
            {
                new(agentId, (WeekDay)date.DayOfWeek, new TimeOnly(8, 0), new TimeOnly(12, 0))
            });

        appointmentRepository
            .Setup(r => r.HasConflictAsync(agentId, date, new TimeOnly(8, 0), new TimeOnly(12, 0), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        appointmentRepository
            .Setup(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
            .Callback<Appointment, CancellationToken>((appointment, _) =>
            {
                EntityTestFactory.PopulateAppointmentNavigations(appointment);
                persistedAppointment = appointment;
            })
            .Returns(Task.CompletedTask);

        appointmentRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => persistedAppointment);

        CreateAppointmentCommandHandler handler = new(
            appointmentRepository.Object,
            availabilityRepository.Object,
            userRepository.Object,
            serviceTypeRepository.Object);

        AppointmentDto result = await handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(AppointmentStatus.PendingConfirmation);
        result.AgentId.Should().Be(agentId);
        result.ClientId.Should().Be(clientId);
        result.ServiceTypeDescription.Should().Be("Consulting");
        appointmentRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AgentNotFound_ThrowsKeyNotFoundException()
    {
        Mock<IAppointmentRepository> appointmentRepository = new();
        Mock<IAgentAvailabilityRepository> availabilityRepository = new();
        Mock<IUserRepository> userRepository = new();
        Mock<IServiceTypeRepository> serviceTypeRepository = new();

        Guid agentId = Guid.NewGuid();

        userRepository
            .Setup(r => r.GetByIdAsync(agentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        CreateAppointmentCommand command = new(
            "Consulta",
            null,
            1,
            Guid.NewGuid(),
            agentId,
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)),
            new TimeOnly(10, 0),
            null);

        CreateAppointmentCommandHandler handler = new(
            appointmentRepository.Object,
            availabilityRepository.Object,
            userRepository.Object,
            serviceTypeRepository.Object);

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Agent '{agentId}' not found.");
    }

    [Fact]
    public async Task Handle_AgentWithConflict_ThrowsInvalidOperationException()
    {
        Mock<IAppointmentRepository> appointmentRepository = new();
        Mock<IAgentAvailabilityRepository> availabilityRepository = new();
        Mock<IUserRepository> userRepository = new();
        Mock<IServiceTypeRepository> serviceTypeRepository = new();

        Guid agentId = Guid.NewGuid();
        DateOnly date = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1));
        TimeOnly time = new(10, 0);

        userRepository
            .Setup(r => r.GetByIdAsync(agentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User("Agent", "agent@email.com", "hash", UserRole.Agent));

        serviceTypeRepository
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ServiceType("Consulting"));

        availabilityRepository
            .Setup(r => r.GetByAgentAndWeekDayAsync(agentId, (WeekDay)date.DayOfWeek, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AgentAvailability>
            {
                new(agentId, (WeekDay)date.DayOfWeek, new TimeOnly(9, 0), new TimeOnly(11, 0))
            });

        appointmentRepository
            .Setup(r => r.HasConflictAsync(agentId, date, new TimeOnly(9, 0), new TimeOnly(11, 0), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        CreateAppointmentCommand command = new(
            "Consulta",
            null,
            1,
            Guid.NewGuid(),
            agentId,
            date,
            time,
            null);

        CreateAppointmentCommandHandler handler = new(
            appointmentRepository.Object,
            availabilityRepository.Object,
            userRepository.Object,
            serviceTypeRepository.Object);

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("The agent already has an appointment at this time.");
    }
}
