using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AgendeX.WebAPI.Middlewares;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Unhandled exception. Method: {Method}, Path: {Path}, TraceId: {TraceId}",
            context.Request.Method,
            context.Request.Path,
            context.TraceIdentifier);

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
