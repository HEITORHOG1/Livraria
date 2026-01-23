using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using Livraria.Application.Mappings;
using Livraria.Domain.Interfaces.Repositories;
using MediatR;

namespace Livraria.Application.Assuntos.Queries.GetAllAssuntos;

/// <summary>
/// Handler para a query de busca de todos os assuntos.
/// </summary>
public class GetAllAssuntosQueryHandler : IRequestHandler<GetAllAssuntosQuery, Result<IEnumerable<AssuntoDto>>>
{
    private readonly IAssuntoRepository _assuntoRepository;

    public GetAllAssuntosQueryHandler(IAssuntoRepository assuntoRepository)
    {
        _assuntoRepository = assuntoRepository;
    }

    public async Task<Result<IEnumerable<AssuntoDto>>> Handle(GetAllAssuntosQuery query, CancellationToken ct)
    {
        var assuntos = await _assuntoRepository.GetAllAsync(ct);
        var assuntosDto = assuntos.Select(a => a.ToDto());
        return Result<IEnumerable<AssuntoDto>>.Success(assuntosDto);
    }
}