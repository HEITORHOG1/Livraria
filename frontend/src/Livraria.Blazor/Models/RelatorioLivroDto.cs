namespace Livraria.Blazor.Models;

/// <summary>
/// DTO para representação dos dados do relatório de livros por autor.
/// </summary>
public record RelatorioLivroDto(
    int CodAu,
    string Autor,
    int CodL,
    string Titulo,
    string Editora,
    int Edicao,
    string AnoPublicacao,
    string? Assuntos
);