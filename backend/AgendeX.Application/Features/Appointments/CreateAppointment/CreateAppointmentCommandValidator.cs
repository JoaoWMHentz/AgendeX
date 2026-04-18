using FluentValidation;

namespace AgendeX.Application.Features.Appointments;

public sealed class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(c => c.Title).NotEmpty().MaximumLength(200);
        RuleFor(c => c.ServiceTypeId).GreaterThan(0);
        RuleFor(c => c.ClientId).NotEmpty();
        RuleFor(c => c.AgentId).NotEmpty();
        RuleFor(c => c.Date)
            .Must(d => d >= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Date cannot be in the past.");
    }
}
