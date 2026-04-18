using AgendeX.Application.Features.Auth.Commands.Login;
using AgendeX.Application.Features.Auth.Commands.Logout;
using AgendeX.Application.Features.Auth.Commands.RefreshToken;
using AgendeX.Application.Features.Auth.Common;
using AgendeX.WebAPI.Models.Auth;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendeX.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            AuthResponseDto response = await _sender.Send(new LoginCommand(request.Email, request.Password), cancellationToken);
            return Ok(response);
        }
        catch (ValidationException exception)
        {
            return BadRequest(new { errors = exception.Errors.Select(error => error.ErrorMessage) });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        try
        {
            AuthResponseDto response = await _sender.Send(new RefreshTokenCommand(request.RefreshToken), cancellationToken);
            return Ok(response);
        }
        catch (ValidationException exception)
        {
            return BadRequest(new { errors = exception.Errors.Select(error => error.ErrorMessage) });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Invalid refresh token." });
        }
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _sender.Send(new LogoutCommand(request.RefreshToken), cancellationToken);
            return NoContent();
        }
        catch (ValidationException exception)
        {
            return BadRequest(new { errors = exception.Errors.Select(error => error.ErrorMessage) });
        }
    }
}
