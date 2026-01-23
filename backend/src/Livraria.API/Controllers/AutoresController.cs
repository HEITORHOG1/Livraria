using Livraria.Application.Autores.Commands.CreateAutor;
using Livraria.Application.Autores.Commands.DeleteAutor;
using Livraria.Application.Autores.Commands.UpdateAutor;
using Livraria.Application.Autores.Queries.GetAllAutores;
using Livraria.Application.Autores.Queries.GetAutorById;
using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Livraria.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AutoresController : ControllerBase
{
    private readonly IMediator _mediator;

    public AutoresController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AutorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllAutoresQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : HandleError(result.Error!);
    }

    [HttpGet("{codAu:int}")]
    [ProducesResponseType(typeof(AutorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int codAu, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAutorByIdQuery(codAu), ct);
        return result.IsSuccess ? Ok(result.Value) : HandleError(result.Error!);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AutorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAutorCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { codAu = result.Value!.CodAu }, result.Value)
            : HandleError(result.Error!);
    }

    [HttpPut("{codAu:int}")]
    [ProducesResponseType(typeof(AutorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int codAu, [FromBody] UpdateAutorCommand command, CancellationToken ct)
    {
        if (codAu != command.CodAu)
            return BadRequest(new { message = "CodAu na URL não corresponde ao corpo da requisição" });

        var result = await _mediator.Send(command, ct);
        return result.IsSuccess ? Ok(result.Value) : HandleError(result.Error!);
    }

    [HttpDelete("{codAu:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int codAu, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteAutorCommand(codAu), ct);
        return result.IsSuccess ? NoContent() : HandleError(result.Error!);
    }

    private IActionResult HandleError(Error error) => error.Code switch
    {
        "NOT_FOUND" => NotFound(new { message = error.Message }),
        "VALIDATION_ERROR" => BadRequest(new { message = error.Message }),
        "CONFLICT" => Conflict(new { message = error.Message }),
        _ => StatusCode(500, new { message = error.Message })
    };
}