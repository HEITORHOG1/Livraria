using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using Livraria.Application.Mappings;
using Livraria.Domain.Interfaces.Repositories;
using MediatR;

namespace Livraria.Application.Livros.Queries.GetLivroById;

/// <summary>
/// Handler para a query de busca de livro por código.
/// </summary>
public class GetLivroByIdQueryHandler : IRequestHandler<GetLivroByIdQuery, Result<LivroDto>>
{
    private readonly ILivroRepository _livroRepository;

    public GetLivroByIdQueryHandler(ILivroRepository livroRepository)
    {
        _livroRepository = livroRepository;
    }

    public async Task<Result<LivroDto>> Handle(GetLivroByIdQuery query, CancellationToken ct)
    {
        var livro = await _livroRepository.GetByIdWithRelationsAsync(query.CodL, ct);

        if (livro is null)
            return Result<LivroDto>.Failure(Error.NotFound($"Livro com código {query.CodL} não encontrado"));

        return Result<LivroDto>.Success(livro.ToDto());
    }
}