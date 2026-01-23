using Livraria.Application.DTOs;
using Livraria.Domain.Entities;

namespace Livraria.Application.Mappings;

/// <summary>
/// Extensões para mapeamento de entidades para DTOs.
/// </summary>
public static class MappingExtensions
{
    public static AutorDto ToDto(this Autor autor)
        => new(autor.CodAu, autor.Nome);

    public static AssuntoDto ToDto(this Assunto assunto)
        => new(assunto.CodAs, assunto.Descricao);

    public static FormaCompraDto ToDto(this FormaCompra formaCompra)
        => new(formaCompra.CodFc, formaCompra.Descricao);

    public static LivroPrecoDto ToDto(this LivroPreco livroPreco)
        => new(
            livroPreco.FormaCompra_CodFc,
            livroPreco.FormaCompra?.Descricao ?? string.Empty,
            livroPreco.Valor
        );

    public static LivroDto ToDto(this Livro livro)
        => new(
            livro.CodL,
            livro.Titulo,
            livro.Editora,
            livro.Edicao,
            livro.AnoPublicacao,
            livro.LivroAutores.Select(la => la.Autor.ToDto()),
            livro.LivroAssuntos.Select(la => la.Assunto.ToDto()),
            livro.LivroPrecos.Select(lp => lp.ToDto())
        );

    public static LivroListDto ToListDto(this Livro livro)
        => new(
            livro.CodL,
            livro.Titulo,
            livro.Editora,
            livro.Edicao,
            livro.AnoPublicacao,
            livro.LivroAutores.Select(la => la.Autor.Nome),
            livro.LivroAssuntos.Select(la => la.Assunto.Descricao)
        );
}