namespace Livraria.Blazor.Models;

/// <summary>
/// DTO para representação do preço de um livro por forma de compra.
/// </summary>
public record LivroPrecoDto(int FormaCompra_CodFc, string FormaCompraDescricao, decimal Valor);