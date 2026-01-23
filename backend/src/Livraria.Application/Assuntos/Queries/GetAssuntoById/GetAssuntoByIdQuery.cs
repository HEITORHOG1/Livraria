using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using MediatR;

namespace Livraria.Application.Assuntos.Queries.GetAssuntoById;

/// <summary>
/// Query para buscar um assunto por código.
/// </summary>
public record GetAssuntoByIdQuery(int CodAs) : IRequest<Result<AssuntoDto>>;