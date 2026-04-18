using FluentValidation;

namespace AgendeX.Application.Features.Appointments;

public sealed class ReassignAppointmentCommandValidator : AbstractValidator<ReassignAppointmentCommand>
{
    public ReassignAppointmentCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.NewAgentId).NotEmpty();
    }
}
