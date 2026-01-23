using Livraria.Domain.Exceptions;

namespace Livraria.Domain.Entities;

/// <summary>
/// Entidade que representa uma forma de compra (canal de venda) no sistema.
/// </summary>
public class FormaCompra
{
    public int CodFc { get; private set; }
    public string Descricao { get; private set; } = string.Empty;

    public ICollection<LivroPreco> LivroPrecos { get; private set; } = new List<LivroPreco>();

    private FormaCompra()
    { } // EF Core

    public static FormaCompra Create(string descricao)
    {
        ValidateDescricao(descricao);
        return new FormaCompra { Descricao = descricao.Trim() };
    }

    private static void ValidateDescricao(string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException("Descrição é obrigatória");
        if (descricao.Length > 20)
            throw new DomainException("Descrição deve ter no máximo 20 caracteres");
    }
}