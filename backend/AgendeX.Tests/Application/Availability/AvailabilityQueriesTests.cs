using AgendeX.Application.Features.Availability;
using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace AgendeX.Tests.Application.Availability;

public sealed class AvailabilityQueriesTests
{
    [Fact]
    public async Task GetAvailabilitiesByAgentHandle_ReturnsMappedDtos()
    {
        Mock<IAgentAvailabilityRepository> repository = new();

        Guid agentId = Guid.NewGuid();
        List<AgentAvailability> availabilities =
        [
            new(agentId, WeekDay.Monday, new TimeOnly(8, 0), new TimeOnly(10, 0)),
            new(agentId, WeekDay.Tuesday, new TimeOnly(14, 0), new TimeOnly(16, 0))
        ];

        repository
            .Setup(r => r.GetByAgentIdAsync(agentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(availabilities);

        GetAvailabilitiesByAgentQueryHandler handler = new(repository.Object);

        IReadOnlyList<AvailabilityDto> result = await handler.Handle(new GetAvailabilitiesByAgentQuery(agentId), CancellationToken.None);

        result.Should().HaveCount(2);
        result.First().AgentId.Should().Be(agentId);
    }

    [Fact]
    public async Task GetAvailableSlotsHandle_FiltersOccupiedAndInactiveSlots()
    {
        Mock<IAgentAvailabilityRepository> availabilityRepository = new();
        Mock<IAppointmentRepository> appointmentRepository = new();

        Guid agentId = Guid.NewGuid();
        DateOnly date = DateOnly.FromDateTime(new DateTime(2026, 4, 20));
        WeekDay weekDay = (WeekDay)date.DayOfWeek;

        AgentAvailability activeFree = new(agentId, weekDay, new TimeOnly(8, 0), new TimeOnly(9, 0));
        AgentAvailability activeOccupied = new(agentId, weekDay, new TimeOnly(9, 0), new TimeOnly(10, 0));
        AgentAvailability inactive = new(agentId, weekDay, new TimeOnly(10, 0), new TimeOnly(11, 0));
        inactive.Deactivate();

        availabilityRepository
            .Setup(r => r.GetByAgentAndWeekDayAsync(agentId, weekDay, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AgentAvailability> { activeFree, activeOccupied, inactive });

        List<Appointment> appointments =
        [
            new Appointment("A", null, 1, Guid.NewGuid(), agentId, date, new TimeOnly(9, 0), null),
            new Appointment("B", null, 1, Guid.NewGuid(), agentId, date, new TimeOnly(10, 0), null)
        ];

        appointments[0].Confirm();
        appointments[1].Cancel();

        appointmentRepository
            .Setup(r => r.GetAllAsync(null, agentId, null, null, date, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointments);

        GetAvailableSlotsQueryHandler handler = new(availabilityRepository.Object, appointmentRepository.Object);

        IReadOnlyList<AvailableSlotDto> result = await handler.Handle(new GetAvailableSlotsQuery(agentId, date), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].StartTime.Should().Be(new TimeOnly(8, 0));
    }

    [Fact]
    public async Task GetAvailableSlotsHandle_NoAvailabilityForDay_ReturnsEmptyList()
    {
        Mock<IAgentAvailabilityRepository> availabilityRepository = new();
        Mock<IAppointmentRepository> appointmentRepository = new();

        Guid agentId = Guid.NewGuid();
        DateOnly date = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        WeekDay weekDay = (WeekDay)date.DayOfWeek;

        availabilityRepository
            .Setup(r => r.GetByAgentAndWeekDayAsync(agentId, weekDay, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AgentAvailability>());

        appointmentRepository
            .Setup(r => r.GetAllAsync(null, agentId, null, null, date, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Appointment>());

        GetAvailableSlotsQueryHandler handler = new(availabilityRepository.Object, appointmentRepository.Object);

        IReadOnlyList<AvailableSlotDto> result = await handler.Handle(new GetAvailableSlotsQuery(agentId, date), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
