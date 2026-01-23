using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using MediatR;

namespace Livraria.Application.Assuntos.Queries.GetAllAssuntos;

/// <summary>
/// Query para buscar todos os assuntos.
/// </summary>
public record GetAllAssuntosQuery : IRequest<Result<IEnumerable<AssuntoDto>>>;