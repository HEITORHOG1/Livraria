using Livraria.Application.Common.Interfaces;
using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using Livraria.Application.Relatorios.Queries.GetRelatorioLivrosPorAutor;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Livraria.API.Controllers;

/// <summary>
/// Controller para endpoints de relatórios.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RelatoriosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IRelatorioService _relatorioService;

    public RelatoriosController(IMediator mediator, IRelatorioService relatorioService)
    {
        _mediator = mediator;
        _relatorioService = relatorioService;
    }

    /// <summary>
    /// Obtém os dados do relatório de livros agrupados por autor.
    /// </summary>
    [HttpGet("livros-por-autor")]
    [ProducesResponseType(typeof(IEnumerable<RelatorioLivroDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRelatorioLivrosPorAutor(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetRelatorioLivrosPorAutorQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : HandleError(result.Error!);
    }

    /// <summary>
    /// Gera e retorna o PDF do relatório de livros agrupados por autor.
    /// </summary>
    /// <param name="autorIds">IDs dos autores para filtrar (opcional). Se não informado, inclui todos.</param>
    [HttpGet("livros-por-autor/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRelatorioLivrosPorAutorPdf([FromQuery] int[]? autorIds, CancellationToken ct)
    {
        var pdf = await _relatorioService.GerarPdfAsync(autorIds, ct);

        var fileName = autorIds != null && autorIds.Length > 0
            ? "relatorio-livros-filtrado.pdf"
            : "relatorio-livros-por-autor.pdf";

        return File(pdf, "application/pdf", fileName);
    }

    private IActionResult HandleError(Error error) => error.Code switch
    {
        "NOT_FOUND" => NotFound(new { message = error.Message }),
        "VALIDATION_ERROR" => BadRequest(new { message = error.Message }),
        "CONFLICT" => Conflict(new { message = error.Message }),
        _ => StatusCode(500, new { message = error.Message })
    };
}