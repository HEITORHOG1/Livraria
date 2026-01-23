using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using MediatR;

namespace Livraria.Application.Livros.Queries.GetAllLivros;

/// <summary>
/// Query para buscar todos os livros.
/// </summary>
public record GetAllLivrosQuery : IRequest<Result<IEnumerable<LivroListDto>>>;