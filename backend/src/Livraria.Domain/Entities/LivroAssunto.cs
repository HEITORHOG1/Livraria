namespace Livraria.Domain.Entities;

/// <summary>
/// Entidade de junção que representa o relacionamento N:N entre Livro e Assunto.
/// </summary>
public class LivroAssunto
{
    public int Livro_CodL { get; set; }
    public int Assunto_CodAs { get; set; }

    public Livro Livro { get; set; } = null!;
    public Assunto Assunto { get; set; } = null!;
}