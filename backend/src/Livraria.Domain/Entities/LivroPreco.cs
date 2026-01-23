using Livraria.Domain.Exceptions;

namespace Livraria.Domain.Entities;

/// <summary>
/// Entidade de junção que representa o preço de um livro por forma de compra.
/// </summary>
public class LivroPreco
{
    public int Livro_CodL { get; set; }
    public int FormaCompra_CodFc { get; set; }
    public decimal Valor { get; set; }

    public Livro Livro { get; set; } = null!;
    public FormaCompra FormaCompra { get; set; } = null!;

    public static void ValidateValor(decimal valor)
    {
        if (valor < 0)
            throw new DomainException("Valor não pode ser negativo");
    }
}