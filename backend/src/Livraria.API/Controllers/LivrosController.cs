using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using Livraria.Application.Livros.Commands.CreateLivro;
using Livraria.Application.Livros.Commands.DeleteLivro;
using Livraria.Application.Livros.Commands.UpdateLivro;
using Livraria.Application.Livros.Queries.GetAllLivros;
using Livraria.Application.Livros.Queries.GetLivroById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Livraria.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LivrosController : ControllerBase
{
    private readonly IMediator _mediator;

    public LivrosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LivroListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllLivrosQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : HandleError(result.Error!);
    }

    [HttpGet("{codL:int}")]
    [ProducesResponseType(typeof(LivroDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int codL, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLivroByIdQuery(codL), ct);
        return result.IsSuccess ? Ok(result.Value) : HandleError(result.Error!);
    }

    [HttpPost]
    [ProducesResponseType(typeof(LivroDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateLivroCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { codL = result.Value!.CodL }, result.Value)
            : HandleError(result.Error!);
    }

    [HttpPut("{codL:int}")]
    [ProducesResponseType(typeof(LivroDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int codL, [FromBody] UpdateLivroCommand command, CancellationToken ct)
    {
        if (codL != command.CodL)
            return BadRequest(new { message = "CodL na URL não corresponde ao corpo da requisição" });

        var result = await _mediator.Send(command, ct);
        return result.IsSuccess ? Ok(result.Value) : HandleError(result.Error!);
    }

    [HttpDelete("{codL:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int codL, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteLivroCommand(codL), ct);
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