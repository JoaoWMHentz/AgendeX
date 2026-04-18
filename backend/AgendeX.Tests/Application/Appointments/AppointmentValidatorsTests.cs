using AgendeX.Application.Features.Appointments;
using FluentAssertions;

namespace AgendeX.Tests.Application.Appointments;

public sealed class AppointmentValidatorsTests
{
    [Fact]
    public void CreateAppointmentCommandValidator_DateInPast_ShouldBeInvalid()
    {
        CreateAppointmentCommandValidator validator = new();
        CreateAppointmentCommand command = new(
            "Consulta",
            null,
            1,
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1)),
            new TimeOnly(10, 0),
            null);

        FluentValidation.Results.ValidationResult result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Date cannot be in the past.");
    }

    [Fact]
    public void RejectAppointmentCommandValidator_EmptyReason_ShouldBeInvalid()
    {
        RejectAppointmentCommandValidator validator = new();
        RejectAppointmentCommand command = new(Guid.NewGuid(), Guid.NewGuid(), string.Empty);

        FluentValidation.Results.ValidationResult result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RejectionReason");
    }

    [Fact]
    public void ReassignAppointmentCommandValidator_EmptyNewAgentId_ShouldBeInvalid()
    {
        ReassignAppointmentCommandValidator validator = new();
        ReassignAppointmentCommand command = new(Guid.NewGuid(), Guid.Empty);

        FluentValidation.Results.ValidationResult result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewAgentId");
    }
}
