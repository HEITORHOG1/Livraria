using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using Livraria.Application.Mappings;
using Livraria.Domain.Interfaces.Repositories;
using MediatR;

namespace Livraria.Application.FormasCompra.Queries.GetAllFormasCompra;

/// <summary>
/// Handler para a query de busca de todas as formas de compra.
/// </summary>
public class GetAllFormasCompraQueryHandler : IRequestHandler<GetAllFormasCompraQuery, Result<IEnumerable<FormaCompraDto>>>
{
    private readonly IFormaCompraRepository _formaCompraRepository;

    public GetAllFormasCompraQueryHandler(IFormaCompraRepository formaCompraRepository)
    {
        _formaCompraRepository = formaCompraRepository;
    }

    public async Task<Result<IEnumerable<FormaCompraDto>>> Handle(GetAllFormasCompraQuery query, CancellationToken ct)
    {
        var formasCompra = await _formaCompraRepository.GetAllAsync(ct);
        var formasCompraDto = formasCompra.Select(f => f.ToDto());
        return Result<IEnumerable<FormaCompraDto>>.Success(formasCompraDto);
    }
}