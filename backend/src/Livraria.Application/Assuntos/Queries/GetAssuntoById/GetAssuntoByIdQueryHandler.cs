using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using Livraria.Application.Mappings;
using Livraria.Domain.Interfaces.Repositories;
using MediatR;

namespace Livraria.Application.Assuntos.Queries.GetAssuntoById;

/// <summary>
/// Handler para a query de busca de assunto por código.
/// </summary>
public class GetAssuntoByIdQueryHandler : IRequestHandler<GetAssuntoByIdQuery, Result<AssuntoDto>>
{
    private readonly IAssuntoRepository _assuntoRepository;

    public GetAssuntoByIdQueryHandler(IAssuntoRepository assuntoRepository)
    {
        _assuntoRepository = assuntoRepository;
    }

    public async Task<Result<AssuntoDto>> Handle(GetAssuntoByIdQuery query, CancellationToken ct)
    {
        var assunto = await _assuntoRepository.GetByIdAsync(query.CodAs, ct);

        if (assunto is null)
            return Result<AssuntoDto>.Failure(Error.NotFound($"Assunto com código {query.CodAs} não encontrado"));

        return Result<AssuntoDto>.Success(assunto.ToDto());
    }
}