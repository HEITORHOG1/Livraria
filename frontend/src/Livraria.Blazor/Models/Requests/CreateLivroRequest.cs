namespace Livraria.Blazor.Models.Requests;

/// <summary>
/// Request para criar um novo livro.
/// </summary>
public record CreateLivroRequest(
    string Titulo,
    string Editora,
    int Edicao,
    string AnoPublicacao,
    int[] AutoresCodAu,
    int[] AssuntosCodAs,
    Dictionary<int, decimal> Precos
);