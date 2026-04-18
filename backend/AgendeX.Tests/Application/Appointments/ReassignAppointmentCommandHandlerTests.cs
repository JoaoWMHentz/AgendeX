using AgendeX.Application.Features.Appointments;
using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using AgendeX.Tests.Application.Common;
using FluentAssertions;
using Moq;

namespace AgendeX.Tests.Application.Appointments;

public sealed class ReassignAppointmentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidRequest_UpdatesAgentAndReturnsDto()
    {
        Mock<IAppointmentRepository> appointmentRepository = new();
        Mock<IUserRepository> userRepository = new();

        Guid newAgentId = Guid.NewGuid();
        Appointment appointment = EntityTestFactory.CreateAppointment(status: AppointmentStatus.PendingConfirmation);

        appointmentRepository
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        userRepository
            .Setup(r => r.GetByIdAsync(newAgentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User("Agent", "agent@email.com", "hash", UserRole.Agent));

        appointmentRepository
            .Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id == appointment.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                EntityTestFactory.PopulateAppointmentNavigations(appointment);
                return appointment;
            });

        ReassignAppointmentCommandHandler handler = new(appointmentRepository.Object, userRepository.Object);

        AppointmentDto result = await handler.Handle(
            new ReassignAppointmentCommand(appointment.Id, newAgentId),
            CancellationToken.None);

        result.AgentId.Should().Be(newAgentId);
        appointmentRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NewAgentNotFound_ThrowsKeyNotFoundException()
    {
        Mock<IAppointmentRepository> appointmentRepository = new();
        Mock<IUserRepository> userRepository = new();

        Appointment appointment = EntityTestFactory.CreateAppointment();
        Guid newAgentId = Guid.NewGuid();

        appointmentRepository
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        userRepository
            .Setup(r => r.GetByIdAsync(newAgentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        ReassignAppointmentCommandHandler handler = new(appointmentRepository.Object, userRepository.Object);

        Func<Task> act = async () => await handler.Handle(
            new ReassignAppointmentCommand(appointment.Id, newAgentId),
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Agent '{newAgentId}' not found.");
    }

    [Fact]
    public async Task Handle_CompletedAppointment_ThrowsInvalidOperationException()
    {
        Mock<IAppointmentRepository> appointmentRepository = new();
        Mock<IUserRepository> userRepository = new();

        Guid newAgentId = Guid.NewGuid();
        Appointment appointment = EntityTestFactory.CreateAppointment(status: AppointmentStatus.Completed);

        appointmentRepository
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        userRepository
            .Setup(r => r.GetByIdAsync(newAgentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User("Agent", "agent@email.com", "hash", UserRole.Agent));

        ReassignAppointmentCommandHandler handler = new(appointmentRepository.Object, userRepository.Object);

        Func<Task> act = async () => await handler.Handle(
            new ReassignAppointmentCommand(appointment.Id, newAgentId),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot reassign a completed or canceled appointment.");
    }
}
