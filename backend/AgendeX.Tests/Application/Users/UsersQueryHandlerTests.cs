using AgendeX.Application.Features.Users;
using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace AgendeX.Tests.Application.Users;

public sealed class UsersQueryHandlerTests
{
    [Fact]
    public async Task GetUsersQueryHandler_ReturnsMappedDtos()
    {
        Mock<IUserRepository> userRepository = new();
        User user = new("Ana", "ana@example.com", "hash", UserRole.Administrator);

        userRepository
            .Setup(r => r.GetAllAsync(UserRole.Administrator, It.IsAny<CancellationToken>()))
            .ReturnsAsync([user]);

        GetUsersQueryHandler handler = new(userRepository.Object);

        IReadOnlyList<UserDto> result = await handler.Handle(new GetUsersQuery(UserRole.Administrator), CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Id.Should().Be(user.Id);
        result[0].Name.Should().Be(user.Name);
        result[0].Email.Should().Be(user.Email);
        result[0].Role.Should().Be(user.Role);
        result[0].IsActive.Should().BeTrue();
        result[0].CreatedAt.Should().Be(user.CreatedAt);
        result[0].ClientDetail.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByIdQueryHandler_ExistingUser_ReturnsDto()
    {
        Mock<IUserRepository> userRepository = new();
        User user = new("Bruno", "bruno@example.com", "hash", UserRole.Agent);

        userRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        GetUserByIdQueryHandler handler = new(userRepository.Object);

        UserDto result = await handler.Handle(new GetUserByIdQuery(user.Id), CancellationToken.None);

        result.Id.Should().Be(user.Id);
        result.Name.Should().Be(user.Name);
        result.Email.Should().Be(user.Email);
        result.Role.Should().Be(user.Role);
    }

    [Fact]
    public async Task GetUserByIdQueryHandler_MissingUser_ThrowsKeyNotFoundException()
    {
        Mock<IUserRepository> userRepository = new();
        Guid userId = Guid.NewGuid();

        userRepository
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        GetUserByIdQueryHandler handler = new(userRepository.Object);

        Func<Task> act = () => handler.Handle(new GetUserByIdQuery(userId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"User '{userId}' not found.");
    }

    [Fact]
    public async Task GetAgentsQueryHandler_ReturnsActiveAgentsAsLookupDtos()
    {
        Mock<IUserRepository> userRepository = new();
        User agent = new("Carla", "carla@example.com", "hash", UserRole.Agent);

        userRepository
            .Setup(r => r.GetActiveAgentsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([agent]);

        GetAgentsQueryHandler handler = new(userRepository.Object);

        IReadOnlyList<AgentLookupDto> result = await handler.Handle(new GetAgentsQuery(), CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Id.Should().Be(agent.Id);
        result[0].Name.Should().Be(agent.Name);
    }
}