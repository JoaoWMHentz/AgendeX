using AgendeX.Application.Features.Appointments;
using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace AgendeX.Tests.Application.Appointments;

public sealed class GetAppointmentByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_AppointmentExists_ReturnsDto()
    {
        User agent = new("Bob", "bob@email.com", "hash", UserRole.Agent);
        User client = new("Alice", "alice@email.com", "hash", UserRole.Client);
        
        Appointment appointment = new("Service", "Description", 1, client.Id, agent.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), new TimeOnly(14, 0), "Notes");

        Mock<IAppointmentRepository> repositoryMock = new();

        repositoryMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        GetAppointmentByIdQueryHandler handler = new(repositoryMock.Object);

        AppointmentDto result = await handler.Handle(
            new GetAppointmentByIdQuery(appointment.Id),
            CancellationToken.None);

        result.Id.Should().Be(appointment.Id);
        result.Title.Should().Be("Service");
        result.Status.Should().Be(AppointmentStatus.PendingConfirmation);
    }

    [Fact]
    public async Task Handle_AppointmentNotFound_ThrowsKeyNotFoundException()
    {
        Mock<IAppointmentRepository> repositoryMock = new();

        repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        GetAppointmentByIdQueryHandler handler = new(repositoryMock.Object);

        Func<Task> action = async () => await handler.Handle(
            new GetAppointmentByIdQuery(Guid.NewGuid()),
            CancellationToken.None);

        await action.Should().ThrowAsync<KeyNotFoundException>();
    }
}

public sealed class GetAppointmentsQueryHandlerTests
{
    [Fact]
    public async Task Handle_NoFilters_ReturnsAllAppointments()
    {
        User agent = new("Bob", "bob@email.com", "hash", UserRole.Agent);
        User client = new("Alice", "alice@email.com", "hash", UserRole.Client);
        
        Appointment appointment1 = new("Service 1", null, 1, client.Id, agent.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), new TimeOnly(14, 0), null);
        Appointment appointment2 = new("Service 2", null, 1, client.Id, agent.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(6)), new TimeOnly(15, 0), null);

        List<Appointment> appointments = [appointment1, appointment2];

        Mock<IAppointmentRepository> repositoryMock = new();

        repositoryMock
            .Setup(r => r.GetAllAsync(
                It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<int?>(),
                It.IsAny<AppointmentStatus?>(), It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointments.AsReadOnly());

        GetAppointmentsQueryHandler handler = new(repositoryMock.Object);

        IReadOnlyList<AppointmentDto> result = await handler.Handle(
            new GetAppointmentsQuery(null, null, null, null, null, null),
            CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithClientFilter_ReturnsOnlyClientAppointments()
    {
        User agent = new("Bob", "bob@email.com", "hash", UserRole.Agent);
        User client1 = new("Alice", "alice@email.com", "hash", UserRole.Client);
        User client2 = new("Diana", "diana@email.com", "hash", UserRole.Client);
        
        Appointment appointment1 = new("Service 1", null, 1, client1.Id, agent.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), new TimeOnly(14, 0), null);

        List<Appointment> appointments = [appointment1];

        Mock<IAppointmentRepository> repositoryMock = new();

        repositoryMock
            .Setup(r => r.GetAllAsync(
                client1.Id, null, null, null, null, null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointments.AsReadOnly());

        GetAppointmentsQueryHandler handler = new(repositoryMock.Object);

        IReadOnlyList<AppointmentDto> result = await handler.Handle(
            new GetAppointmentsQuery(client1.Id, null, null, null, null, null),
            CancellationToken.None);

        result.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ReturnsMatchingAppointments()
    {
        User agent = new("Bob", "bob@email.com", "hash", UserRole.Agent);
        User client = new("Alice", "alice@email.com", "hash", UserRole.Client);
        
        Appointment appointment = new("Service", null, 1, client.Id, agent.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), new TimeOnly(14, 0), null);
        appointment.Confirm();

        List<Appointment> appointments = [appointment];

        Mock<IAppointmentRepository> repositoryMock = new();

        repositoryMock
            .Setup(r => r.GetAllAsync(
                null, null, null, AppointmentStatus.Confirmed, null, null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointments.AsReadOnly());

        GetAppointmentsQueryHandler handler = new(repositoryMock.Object);

        IReadOnlyList<AppointmentDto> result = await handler.Handle(
            new GetAppointmentsQuery(null, null, null, AppointmentStatus.Confirmed, null, null),
            CancellationToken.None);

        result.Should().ContainSingle();
        result.First().Status.Should().Be(AppointmentStatus.Confirmed);
    }
}
