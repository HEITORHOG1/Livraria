using Livraria.Application.Common.Interfaces;
using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using MediatR;

namespace Livraria.Application.Relatorios.Queries.GetRelatorioLivrosPorAutor;

/// <summary>
/// Handler para a query de relatório de livros por autor.
/// Consulta a VIEW vw_Relatorio_Livros_Por_Autor e ordena por Autor, depois por Titulo.
/// </summary>
public class GetRelatorioLivrosPorAutorQueryHandler
    : IRequestHandler<GetRelatorioLivrosPorAutorQuery, Result<IEnumerable<RelatorioLivroDto>>>
{
    private readonly IRelatorioService _relatorioService;

    public GetRelatorioLivrosPorAutorQueryHandler(IRelatorioService relatorioService)
    {
        _relatorioService = relatorioService;
    }

    public async Task<Result<IEnumerable<RelatorioLivroDto>>> Handle(
        GetRelatorioLivrosPorAutorQuery request,
        CancellationToken ct)
    {
        var dados = await _relatorioService.GetDadosRelatorioAsync(ct);

        var dadosOrdenados = dados
            .OrderBy(d => d.Autor)
            .ThenBy(d => d.Titulo);

        return Result<IEnumerable<RelatorioLivroDto>>.Success(dadosOrdenados);
    }
}