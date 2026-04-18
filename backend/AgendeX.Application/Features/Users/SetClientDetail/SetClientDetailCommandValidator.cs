using FluentValidation;

namespace AgendeX.Application.Features.Users;

public sealed class SetClientDetailCommandValidator : AbstractValidator<SetClientDetailCommand>
{
    public SetClientDetailCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.CPF).NotEmpty().MaximumLength(14);
        RuleFor(c => c.BirthDate).NotEmpty();
        RuleFor(c => c.Phone).NotEmpty().MaximumLength(20);
    }
}
