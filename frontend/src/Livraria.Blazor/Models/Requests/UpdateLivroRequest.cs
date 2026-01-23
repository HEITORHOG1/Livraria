namespace Livraria.Blazor.Models.Requests;

/// <summary>
/// Request para atualizar um livro existente.
/// </summary>
public record UpdateLivroRequest(
    int CodL,
    string Titulo,
    string Editora,
    int Edicao,
    string AnoPublicacao,
    int[] AutoresCodAu,
    int[] AssuntosCodAs,
    Dictionary<int, decimal> Precos
);