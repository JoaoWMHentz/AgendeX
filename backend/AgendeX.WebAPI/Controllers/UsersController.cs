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

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    public sealed record CreateUserRequest(
        string Name, string Email, string Password, UserRole Role,
        string? CPF, DateOnly? BirthDate, string? Phone, string? Notes);

    public sealed record UpdateUserRequest(
        string Name, string? CPF, DateOnly? BirthDate, string? Phone, string? Notes);

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] UserRole? role, CancellationToken cancellationToken)
    {
        IReadOnlyList<UserDto> users = await _sender.Send(new GetUsersQuery(role), cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        UserDto user = await _sender.Send(new GetUserByIdQuery(id), cancellationToken);
        return Ok(user);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        UserDto user = await _sender.Send(new CreateUserCommand(
            request.Name, request.Email, request.Password, request.Role,
            request.CPF, request.BirthDate, request.Phone, request.Notes
        ), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        UserDto user = await _sender.Send(new UpdateUserCommand(
            id, request.Name, request.CPF, request.BirthDate, request.Phone, request.Notes
        ), cancellationToken);

        return Ok(user);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteUserCommand(id), cancellationToken);
        return NoContent();
    }
}
