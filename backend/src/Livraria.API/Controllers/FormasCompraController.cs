using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using Livraria.Application.FormasCompra.Queries.GetAllFormasCompra;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Livraria.API.Controllers;

[ApiController]
[Route("api/formas-compra")]
public class FormasCompraController : ControllerBase
{
    private readonly IMediator _mediator;

    public FormasCompraController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FormaCompraDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllFormasCompraQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : HandleError(result.Error!);
    }

    private IActionResult HandleError(Error error) => error.Code switch
    {
        "NOT_FOUND" => NotFound(new { message = error.Message }),
        "VALIDATION_ERROR" => BadRequest(new { message = error.Message }),
        "CONFLICT" => Conflict(new { message = error.Message }),
        _ => StatusCode(500, new { message = error.Message })
    };
}