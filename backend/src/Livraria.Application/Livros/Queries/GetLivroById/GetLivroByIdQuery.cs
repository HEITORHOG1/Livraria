using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using MediatR;

namespace Livraria.Application.Livros.Queries.GetLivroById;

/// <summary>
/// Query para buscar um livro por código.
/// </summary>
public record GetLivroByIdQuery(int CodL) : IRequest<Result<LivroDto>>;