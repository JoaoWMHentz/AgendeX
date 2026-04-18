using AgendeX.Application.Features.ServiceTypes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AgendeX.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ServiceTypesController : ControllerBase
{
    private readonly ISender _sender;

    public ServiceTypesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ServiceTypeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        IReadOnlyList<ServiceTypeDto> types = await _sender.Send(new GetServiceTypesQuery(), cancellationToken);
        return Ok(types);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ServiceTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        ServiceTypeDto type = await _sender.Send(new GetServiceTypeByIdQuery(id), cancellationToken);
        return Ok(type);
    }
}
