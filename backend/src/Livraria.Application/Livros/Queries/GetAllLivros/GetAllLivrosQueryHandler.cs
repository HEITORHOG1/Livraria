using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using Livraria.Application.Mappings;
using Livraria.Domain.Interfaces.Repositories;
using MediatR;

namespace Livraria.Application.Livros.Queries.GetAllLivros;

/// <summary>
/// Handler para a query de busca de todos os livros.
/// </summary>
public class GetAllLivrosQueryHandler : IRequestHandler<GetAllLivrosQuery, Result<IEnumerable<LivroListDto>>>
{
    private readonly ILivroRepository _livroRepository;

    public GetAllLivrosQueryHandler(ILivroRepository livroRepository)
    {
        _livroRepository = livroRepository;
    }

    public async Task<Result<IEnumerable<LivroListDto>>> Handle(GetAllLivrosQuery query, CancellationToken ct)
    {
        var livros = await _livroRepository.GetAllWithRelationsAsync(ct);
        var livrosDto = livros.Select(l => l.ToListDto());
        return Result<IEnumerable<LivroListDto>>.Success(livrosDto);
    }
}