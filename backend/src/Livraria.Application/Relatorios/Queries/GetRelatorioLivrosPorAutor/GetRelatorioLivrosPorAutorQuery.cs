using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using MediatR;

namespace Livraria.Application.Relatorios.Queries.GetRelatorioLivrosPorAutor;

/// <summary>
/// Query para buscar dados do relatório de livros agrupados por autor.
/// </summary>
public record GetRelatorioLivrosPorAutorQuery : IRequest<Result<IEnumerable<RelatorioLivroDto>>>;