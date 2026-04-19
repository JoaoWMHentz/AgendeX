using AgendeX.Application.Features.Availability;
using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace AgendeX.Tests.Application.Availability;

public sealed class CreateAvailabilityCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidRequest_ReturnsAvailabilitiesDtos()
    {
        Mock<IAgentAvailabilityRepository> repository = new();
        Mock<IUserRepository> userRepository = new();

        Guid agentId = Guid.NewGuid();
        CreateAvailabilityCommand command = new(
            agentId,
            new[] { WeekDay.Monday, WeekDay.Wednesday },
            new TimeOnly(8, 0),
            new TimeOnly(12, 0));

        userRepository
            .Setup(r => r.GetByIdAsync(agentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User("Agent", "agent@email.com", "hash", UserRole.Agent));

        repository
            .Setup(r => r.GetByAgentAndWeekDayAsync(agentId, WeekDay.Monday, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AgentAvailability>());
        repository
            .Setup(r => r.GetByAgentAndWeekDayAsync(agentId, WeekDay.Wednesday, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AgentAvailability>());

        CreateAvailabilityCommandHandler handler = new(repository.Object, userRepository.Object);

        IReadOnlyList<AvailabilityDto> result = await handler.Handle(command, CancellationToken.None);

        result.Should().HaveCount(2);
        result.All(a => a.AgentId == agentId).Should().BeTrue();
        result.All(a => a.IsActive).Should().BeTrue();
        repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AgentNotFound_ThrowsKeyNotFoundException()
    {
        Mock<IAgentAvailabilityRepository> repository = new();
        Mock<IUserRepository> userRepository = new();

        Guid agentId = Guid.NewGuid();
        userRepository
            .Setup(r => r.GetByIdAsync(agentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        CreateAvailabilityCommandHandler handler = new(repository.Object, userRepository.Object);

        Func<Task> act = async () => await handler.Handle(
            new CreateAvailabilityCommand(
                agentId,
                new[] { WeekDay.Tuesday },
                new TimeOnly(9, 0),
                new TimeOnly(11, 0)),
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Atendente '{agentId}' nao encontrado.");
    }

    [Fact]
    public async Task Handle_OverlappingInterval_ThrowsInvalidOperationException()
    {
        Mock<IAgentAvailabilityRepository> repository = new();
        Mock<IUserRepository> userRepository = new();

        Guid agentId = Guid.NewGuid();

        userRepository
            .Setup(r => r.GetByIdAsync(agentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User("Agent", "agent@email.com", "hash", UserRole.Agent));

        repository
            .Setup(r => r.GetByAgentAndWeekDayAsync(agentId, WeekDay.Monday, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AgentAvailability>
            {
                new(agentId, WeekDay.Monday, new TimeOnly(10, 0), new TimeOnly(12, 0))
            });

        CreateAvailabilityCommandHandler handler = new(repository.Object, userRepository.Object);

        Func<Task> act = async () => await handler.Handle(
            new CreateAvailabilityCommand(
                agentId,
                new[] { WeekDay.Monday },
                new TimeOnly(11, 0),
                new TimeOnly(13, 0)),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Conflito de disponibilidade na segunda-feira*");
    }
}
