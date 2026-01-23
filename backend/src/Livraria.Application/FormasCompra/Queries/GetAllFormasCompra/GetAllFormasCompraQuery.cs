using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using MediatR;

namespace Livraria.Application.FormasCompra.Queries.GetAllFormasCompra;

/// <summary>
/// Query para buscar todas as formas de compra.
/// </summary>
public record GetAllFormasCompraQuery : IRequest<Result<IEnumerable<FormaCompraDto>>>;