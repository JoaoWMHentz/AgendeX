using AgendeX.Application.Features.Appointments;
using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using AgendeX.Tests.Application.Common;
using FluentAssertions;
using Moq;

namespace AgendeX.Tests.Application.Appointments;

public sealed class AppointmentQueriesTests
{
    [Fact]
    public async Task GetByIdHandle_AppointmentExists_ReturnsDto()
    {
        Mock<IAppointmentRepository> repository = new();
        Appointment appointment = EntityTestFactory.CreateAppointment();

        repository
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        GetAppointmentByIdQueryHandler handler = new(repository.Object);

        AppointmentDto result = await handler.Handle(
            new GetAppointmentByIdQuery(appointment.Id),
            CancellationToken.None);

        result.Id.Should().Be(appointment.Id);
        result.Title.Should().Be("Appointment Title");
    }

    [Fact]
    public async Task GetByIdHandle_AppointmentDoesNotExist_ThrowsKeyNotFoundException()
    {
        Mock<IAppointmentRepository> repository = new();
        Guid appointmentId = Guid.NewGuid();

        repository
            .Setup(r => r.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        GetAppointmentByIdQueryHandler handler = new(repository.Object);

        Func<Task> act = async () => await handler.Handle(new GetAppointmentByIdQuery(appointmentId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Appointment '{appointmentId}' not found.");
    }

    [Fact]
    public async Task GetAllHandle_WithFilters_ReturnsMappedDtosAndForwardsParameters()
    {
        Mock<IAppointmentRepository> repository = new();

        Guid clientId = Guid.NewGuid();
        Guid agentId = Guid.NewGuid();
        DateOnly from = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        DateOnly to = from.AddDays(3);

        List<Appointment> appointments =
        [
            EntityTestFactory.CreateAppointment(clientId: clientId, agentId: agentId, status: AppointmentStatus.PendingConfirmation),
            EntityTestFactory.CreateAppointment(clientId: clientId, agentId: agentId, status: AppointmentStatus.Confirmed)
        ];

        repository
            .Setup(r => r.GetAllAsync(
                clientId,
                agentId,
                1,
                AppointmentStatus.Confirmed,
                from,
                to,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointments);

        GetAppointmentsQueryHandler handler = new(repository.Object);

        IReadOnlyList<AppointmentDto> result = await handler.Handle(
            new GetAppointmentsQuery(clientId, agentId, 1, AppointmentStatus.Confirmed, from, to),
            CancellationToken.None);

        result.Should().HaveCount(2);
        repository.Verify(r => r.GetAllAsync(
            clientId,
            agentId,
            1,
            AppointmentStatus.Confirmed,
            from,
            to,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
