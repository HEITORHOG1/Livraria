namespace Livraria.Blazor.Models;

/// <summary>
/// DTO para representação completa de um livro com todos os relacionamentos.
/// </summary>
public record LivroDto(
    int CodL,
    string Titulo,
    string Editora,
    int Edicao,
    string AnoPublicacao,
    IEnumerable<AutorDto> Autores,
    IEnumerable<AssuntoDto> Assuntos,
    IEnumerable<LivroPrecoDto> Precos
);