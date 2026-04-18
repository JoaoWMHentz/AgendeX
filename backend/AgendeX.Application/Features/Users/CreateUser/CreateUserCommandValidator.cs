using FluentValidation;

namespace AgendeX.Application.Features.Users;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(120);
        RuleFor(c => c.Email).NotEmpty().EmailAddress().MaximumLength(180);
        RuleFor(c => c.Password).NotEmpty().MinimumLength(6);
        RuleFor(c => c.Role).IsInEnum();
    }
}
