using FluentValidation;

namespace AgendeX.Application.Features.Appointments;

public sealed class RejectAppointmentCommandValidator : AbstractValidator<RejectAppointmentCommand>
{
    public RejectAppointmentCommandValidator()
    {
        RuleFor(c => c.RejectionReason).NotEmpty().MaximumLength(500);
    }
}
