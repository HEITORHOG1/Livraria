using Livraria.Domain.Exceptions;

namespace Livraria.Domain.Entities;

/// <summary>
/// Entidade que representa um autor no sistema.
/// </summary>
public class Autor
{
    public int CodAu { get; private set; }
    public string Nome { get; private set; } = string.Empty;

    public ICollection<LivroAutor> LivroAutores { get; private set; } = new List<LivroAutor>();

    private Autor()
    { } // EF Core

    public static Autor Create(string nome)
    {
        ValidateNome(nome);
        return new Autor { Nome = nome.Trim() };
    }

    public void Update(string nome)
    {
        ValidateNome(nome);
        Nome = nome.Trim();
    }

    private static void ValidateNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome é obrigatório");
        if (nome.Length > 40)
            throw new DomainException("Nome deve ter no máximo 40 caracteres");
    }
}