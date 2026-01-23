namespace Livraria.Blazor.Models.Requests;

/// <summary>
/// Request para atualizar um assunto existente.
/// </summary>
public record UpdateAssuntoRequest(int CodAs, string Descricao);