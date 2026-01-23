namespace Livraria.Blazor.Models.Requests;

/// <summary>
/// Request para atualizar um autor existente.
/// </summary>
public record UpdateAutorRequest(int CodAu, string Nome);