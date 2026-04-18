using AgendeX.Application.Features.Availability;
using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace AgendeX.Tests.Application.Availability;

public sealed class UpdateAndDeleteAvailabilityHandlersTests
{
    [Fact]
    public async Task UpdateHandle_ValidRequest_UpdatesTimes()
    {
        Mock<IAgentAvailabilityRepository> repository = new();

        AgentAvailability current = new(Guid.NewGuid(), WeekDay.Wednesday, new TimeOnly(9, 0), new TimeOnly(11, 0));

        repository
            .Setup(r => r.GetByIdAsync(current.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(current);

        repository
            .Setup(r => r.GetByAgentAndWeekDayAsync(current.AgentId, current.WeekDay, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AgentAvailability> { current });

        UpdateAvailabilityCommandHandler handler = new(repository.Object);

        AvailabilityDto result = await handler.Handle(
            new UpdateAvailabilityCommand(current.Id, new TimeOnly(10, 0), new TimeOnly(12, 0)),
            CancellationToken.None);

        result.StartTime.Should().Be(new TimeOnly(10, 0));
        result.EndTime.Should().Be(new TimeOnly(12, 0));
        repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateHandle_OverlappingAnotherSlot_ThrowsInvalidOperationException()
    {
        Mock<IAgentAvailabilityRepository> repository = new();

        Guid agentId = Guid.NewGuid();
        AgentAvailability current = new(agentId, WeekDay.Friday, new TimeOnly(8, 0), new TimeOnly(9, 0));
        AgentAvailability other = new(agentId, WeekDay.Friday, new TimeOnly(10, 0), new TimeOnly(12, 0));

        repository
            .Setup(r => r.GetByIdAsync(current.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(current);

        repository
            .Setup(r => r.GetByAgentAndWeekDayAsync(agentId, WeekDay.Friday, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AgentAvailability> { current, other });

        UpdateAvailabilityCommandHandler handler = new(repository.Object);

        Func<Task> act = async () => await handler.Handle(
            new UpdateAvailabilityCommand(current.Id, new TimeOnly(11, 0), new TimeOnly(13, 0)),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Availability interval overlaps with an existing slot.");
    }

    [Fact]
    public async Task DeleteHandle_ExistingAvailability_DeactivatesAvailability()
    {
        Mock<IAgentAvailabilityRepository> repository = new();
        AgentAvailability availability = new(Guid.NewGuid(), WeekDay.Sunday, new TimeOnly(8, 0), new TimeOnly(10, 0));

        repository
            .Setup(r => r.GetByIdAsync(availability.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(availability);

        DeleteAvailabilityCommandHandler handler = new(repository.Object);

        await handler.Handle(new DeleteAvailabilityCommand(availability.Id), CancellationToken.None);

        availability.IsActive.Should().BeFalse();
        repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteHandle_AvailabilityNotFound_ThrowsKeyNotFoundException()
    {
        Mock<IAgentAvailabilityRepository> repository = new();
        Guid id = Guid.NewGuid();

        repository
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AgentAvailability?)null);

        DeleteAvailabilityCommandHandler handler = new(repository.Object);

        Func<Task> act = async () => await handler.Handle(new DeleteAvailabilityCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Availability '{id}' not found.");
    }
}
