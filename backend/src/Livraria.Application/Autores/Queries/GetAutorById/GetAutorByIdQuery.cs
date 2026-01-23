using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using MediatR;

namespace Livraria.Application.Autores.Queries.GetAutorById;

/// <summary>
/// Query para buscar um autor por código.
/// </summary>
public record GetAutorByIdQuery(int CodAu) : IRequest<Result<AutorDto>>;