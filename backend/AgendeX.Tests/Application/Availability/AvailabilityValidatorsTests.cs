using AgendeX.Application.Features.Availability;
using AgendeX.Domain.Enums;
using FluentAssertions;

namespace AgendeX.Tests.Application.Availability;

public sealed class AvailabilityValidatorsTests
{
    [Fact]
    public void CreateAvailabilityCommandValidator_EndTimeBeforeStartTime_ShouldBeInvalid()
    {
        CreateAvailabilityCommandValidator validator = new();
        CreateAvailabilityCommand command = new(
            Guid.NewGuid(),
            new[] { WeekDay.Wednesday },
            new TimeOnly(11, 0),
            new TimeOnly(10, 0),
            null);

        FluentValidation.Results.ValidationResult result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "EndTime must be after StartTime.");
    }

    [Fact]
    public void CreateAvailabilityCommandValidator_Sunday_ShouldBeInvalid()
    {
        CreateAvailabilityCommandValidator validator = new();
        CreateAvailabilityCommand command = new(
            Guid.NewGuid(),
            new[] { WeekDay.Sunday },
            new TimeOnly(9, 0),
            new TimeOnly(10, 0),
            null);

        FluentValidation.Results.ValidationResult result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Only Monday to Friday is allowed.");
    }

    [Fact]
    public void UpdateAvailabilityCommandValidator_EmptyId_ShouldBeInvalid()
    {
        UpdateAvailabilityCommandValidator validator = new();
        UpdateAvailabilityCommand command = new(
            Guid.Empty,
            new TimeOnly(9, 0),
            new TimeOnly(10, 0));

        FluentValidation.Results.ValidationResult result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }
}
