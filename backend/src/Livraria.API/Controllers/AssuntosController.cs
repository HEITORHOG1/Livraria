using Livraria.Application.Assuntos.Commands.CreateAssunto;
using Livraria.Application.Assuntos.Commands.DeleteAssunto;
using Livraria.Application.Assuntos.Commands.UpdateAssunto;
using Livraria.Application.Assuntos.Queries.GetAllAssuntos;
using Livraria.Application.Assuntos.Queries.GetAssuntoById;
using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Livraria.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssuntosController : ControllerBase
{
    private readonly IMediator _mediator;

    public AssuntosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AssuntoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllAssuntosQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : HandleError(result.Error!);
    }

    [HttpGet("{codAs:int}")]
    [ProducesResponseType(typeof(AssuntoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int codAs, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAssuntoByIdQuery(codAs), ct);
        return result.IsSuccess ? Ok(result.Value) : HandleError(result.Error!);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AssuntoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAssuntoCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { codAs = result.Value!.CodAs }, result.Value)
            : HandleError(result.Error!);
    }

    [HttpPut("{codAs:int}")]
    [ProducesResponseType(typeof(AssuntoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int codAs, [FromBody] UpdateAssuntoCommand command, CancellationToken ct)
    {
        if (codAs != command.CodAs)
            return BadRequest(new { message = "CodAs na URL não corresponde ao corpo da requisição" });

        var result = await _mediator.Send(command, ct);
        return result.IsSuccess ? Ok(result.Value) : HandleError(result.Error!);
    }

    [HttpDelete("{codAs:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int codAs, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteAssuntoCommand(codAs), ct);
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