using Livraria.Domain.Exceptions;

namespace Livraria.Domain.Entities;

/// <summary>
/// Entidade que representa um assunto/categoria de livro no sistema.
/// </summary>
public class Assunto
{
    public int CodAs { get; private set; }
    public string Descricao { get; private set; } = string.Empty;

    public ICollection<LivroAssunto> LivroAssuntos { get; private set; } = new List<LivroAssunto>();

    private Assunto()
    { } // EF Core

    public static Assunto Create(string descricao)
    {
        ValidateDescricao(descricao);
        return new Assunto { Descricao = descricao.Trim() };
    }

    public void Update(string descricao)
    {
        ValidateDescricao(descricao);
        Descricao = descricao.Trim();
    }

    private static void ValidateDescricao(string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException("Descrição é obrigatória");
        if (descricao.Length > 20)
            throw new DomainException("Descrição deve ter no máximo 20 caracteres");
    }
}