using AgendeX.Application.Features.ServiceTypes;
using AgendeX.Domain.Entities;
using AgendeX.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace AgendeX.Tests.Application.ServiceTypes;

public sealed class ServiceTypeQueryHandlerTests
{
    [Fact]
    public async Task GetServiceTypesQueryHandler_ReturnsMappedDtos()
    {
        Mock<IServiceTypeRepository> repository = new();
        ServiceType first = new("Consulta");
        ServiceType second = new("Retorno");

        repository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([first, second]);

        GetServiceTypesQueryHandler handler = new(repository.Object);

        IReadOnlyList<ServiceTypeDto> result = await handler.Handle(new GetServiceTypesQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(first.Id);
        result[0].Description.Should().Be(first.Description);
        result[1].Id.Should().Be(second.Id);
        result[1].Description.Should().Be(second.Description);
    }

    [Fact]
    public async Task GetServiceTypeByIdQueryHandler_ExistingType_ReturnsDto()
    {
        Mock<IServiceTypeRepository> repository = new();
        ServiceType type = new("Exame");

        repository
            .Setup(r => r.GetByIdAsync(type.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(type);

        GetServiceTypeByIdQueryHandler handler = new(repository.Object);

        ServiceTypeDto result = await handler.Handle(new GetServiceTypeByIdQuery(type.Id), CancellationToken.None);

        result.Id.Should().Be(type.Id);
        result.Description.Should().Be(type.Description);
    }

    [Fact]
    public async Task GetServiceTypeByIdQueryHandler_MissingType_ThrowsKeyNotFoundException()
    {
        Mock<IServiceTypeRepository> repository = new();
        int serviceTypeId = 99;

        repository
            .Setup(r => r.GetByIdAsync(serviceTypeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceType?)null);

        GetServiceTypeByIdQueryHandler handler = new(repository.Object);

        Func<Task> act = () => handler.Handle(new GetServiceTypeByIdQuery(serviceTypeId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"ServiceType '{serviceTypeId}' not found.");
    }
}