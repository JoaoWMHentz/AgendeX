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

    // Id vem da rota — só o campo do body
    public sealed record UpdateUserBody(string Name);

    // UserId vem da rota — só os campos do body
    public sealed record SetClientDetailBody(
        string CPF, DateOnly BirthDate, string Phone, string? Notes);

    [HttpGet]
    [Authorize(Roles = Roles.Administrator)]
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
    [Authorize(Roles = Roles.Administrator)]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserCommand command, CancellationToken cancellationToken)
    {
        UserDto user = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateUserBody body, CancellationToken cancellationToken)
    {
        UserDto user = await _sender.Send(new UpdateUserCommand(id, body.Name), cancellationToken);
        return Ok(user);
    }

    [HttpPut("{id:guid}/client-detail")]
    [Authorize(Roles = Roles.Administrator)]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetClientDetail(
        Guid id, [FromBody] SetClientDetailBody body, CancellationToken cancellationToken)
    {
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
        await _sender.Send(new DeleteUserCommand(id), cancellationToken);
        return NoContent();
    }
}
