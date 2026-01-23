using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using Livraria.Application.Mappings;
using Livraria.Domain.Interfaces.Repositories;
using MediatR;

namespace Livraria.Application.Autores.Queries.GetAllAutores;

/// <summary>
/// Handler para a query de busca de todos os autores.
/// </summary>
public class GetAllAutoresQueryHandler : IRequestHandler<GetAllAutoresQuery, Result<IEnumerable<AutorDto>>>
{
    private readonly IAutorRepository _autorRepository;

    public GetAllAutoresQueryHandler(IAutorRepository autorRepository)
    {
        _autorRepository = autorRepository;
    }

    public async Task<Result<IEnumerable<AutorDto>>> Handle(GetAllAutoresQuery query, CancellationToken ct)
    {
        var autores = await _autorRepository.GetAllAsync(ct);
        var autoresDto = autores.Select(a => a.ToDto());
        return Result<IEnumerable<AutorDto>>.Success(autoresDto);
    }
}