namespace Livraria.Blazor.Models;

/// <summary>
/// DTO simplificado para listagem de livros.
/// </summary>
public record LivroListDto(
    int CodL,
    string Titulo,
    string Editora,
    int Edicao,
    string AnoPublicacao,
    IEnumerable<string> Autores,
    IEnumerable<string> Assuntos
);