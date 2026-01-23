using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using MediatR;

namespace Livraria.Application.Autores.Queries.GetAllAutores;

/// <summary>
/// Query para buscar todos os autores.
/// </summary>
public record GetAllAutoresQuery : IRequest<Result<IEnumerable<AutorDto>>>;