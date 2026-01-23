using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using Livraria.Application.Mappings;
using Livraria.Domain.Interfaces.Repositories;
using MediatR;

namespace Livraria.Application.Autores.Queries.GetAutorById;

/// <summary>
/// Handler para a query de busca de autor por código.
/// </summary>
public class GetAutorByIdQueryHandler : IRequestHandler<GetAutorByIdQuery, Result<AutorDto>>
{
    private readonly IAutorRepository _autorRepository;

    public GetAutorByIdQueryHandler(IAutorRepository autorRepository)
    {
        _autorRepository = autorRepository;
    }

    public async Task<Result<AutorDto>> Handle(GetAutorByIdQuery query, CancellationToken ct)
    {
        var autor = await _autorRepository.GetByIdAsync(query.CodAu, ct);

        if (autor is null)
            return Result<AutorDto>.Failure(Error.NotFound($"Autor com código {query.CodAu} não encontrado"));

        return Result<AutorDto>.Success(autor.ToDto());
    }
}