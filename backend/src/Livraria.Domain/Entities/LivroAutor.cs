namespace Livraria.Domain.Entities;

/// <summary>
/// Entidade de junção que representa o relacionamento N:N entre Livro e Autor.
/// </summary>
public class LivroAutor
{
    public int Livro_CodL { get; set; }
    public int Autor_CodAu { get; set; }

    public Livro Livro { get; set; } = null!;
    public Autor Autor { get; set; } = null!;
}