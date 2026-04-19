using AgendeX.Application.Common.Interfaces;
using AgendeX.Application.Features.Users;
using AgendeX.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendeX.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UsersController> _logger;

    public UsersController(ISender sender, ICurrentUserService currentUser, ILogger<UsersController> logger)
    {
        _sender = sender;
        _currentUser = currentUser;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = Roles.Administrator)]
    [ProducesResponseType(typeof(IReadOnlyList<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] UserRole? role, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetAll users requested with role filter {Role}", role?.ToString() ?? "none");
        IReadOnlyList<UserDto> users = await _sender.Send(new GetUsersQuery(role), cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetById user requested for id {Id}", id);
        UserDto user = await _sender.Send(new GetUserByIdQuery(id), cancellationToken);
        return Ok(user);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Administrator)]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Create user requested for email {Email} with role {Role}", command.Email, command.Role);
        UserDto user = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateUserBody body, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Update user requested for id {Id}", id);

        if (!_currentUser.IsAdmin && _currentUser.UserId != id)
            return Forbid();

        // Only admins may change role or active status
        UserRole? role = _currentUser.IsAdmin ? body.Role : null;
        bool? isActive = _currentUser.IsAdmin ? body.IsActive : null;

        UserDto user = await _sender.Send(new UpdateUserCommand(id, body.Name, role, isActive), cancellationToken);
        return Ok(user);
    }

    [HttpPut("{id:guid}/client-detail")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetClientDetail(
        Guid id, [FromBody] SetClientDetailRequest body, CancellationToken cancellationToken)
    {
        _logger.LogInformation("SetClientDetail requested for user {Id}", id);

        if (!_currentUser.IsAdmin && _currentUser.UserId != id)
            return Forbid();

        UserDto user = await _sender.Send(
            new SetClientDetailCommand(id, body.CPF, body.BirthDate, body.Phone, body.Notes),
            cancellationToken);

        return Ok(user);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Delete user requested for id {Id}", id);
        await _sender.Send(new DeleteUserCommand(id), cancellationToken);
        return NoContent();
    }
}
