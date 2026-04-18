using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AgendeX.WebAPI.Middlewares;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails problem = exception switch
        {
            ValidationException ex => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed.",
                Extensions = { ["errors"] = ex.Errors.Select(e => e.ErrorMessage) }
            },
            InvalidOperationException ex => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = ex.Message
            },
            KeyNotFoundException ex => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = ex.Message
            },
            UnauthorizedAccessException ex => new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = ex.Message
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred."
            }
        };

        context.Response.StatusCode = problem.Status!.Value;
        await context.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}
