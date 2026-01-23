using Livraria.Domain.Exceptions;

namespace Livraria.Domain.Entities;

/// <summary>
/// Entidade que representa um livro no sistema.
/// </summary>
public class Livro
{
    public int CodL { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string Editora { get; private set; } = string.Empty;
    public int Edicao { get; private set; }
    public string AnoPublicacao { get; private set; } = string.Empty;

    public ICollection<LivroAutor> LivroAutores { get; private set; } = new List<LivroAutor>();
    public ICollection<LivroAssunto> LivroAssuntos { get; private set; } = new List<LivroAssunto>();
    public ICollection<LivroPreco> LivroPrecos { get; private set; } = new List<LivroPreco>();

    private Livro()
    { } // EF Core

    public static Livro Create(string titulo, string editora, int edicao, string anoPublicacao)
    {
        ValidateTitulo(titulo);
        ValidateEditora(editora);
        ValidateEdicao(edicao);
        ValidateAnoPublicacao(anoPublicacao);

        return new Livro
        {
            Titulo = titulo.Trim(),
            Editora = editora.Trim(),
            Edicao = edicao,
            AnoPublicacao = anoPublicacao.Trim()
        };
    }

    public void Update(string titulo, string editora, int edicao, string anoPublicacao)
    {
        ValidateTitulo(titulo);
        ValidateEditora(editora);
        ValidateEdicao(edicao);
        ValidateAnoPublicacao(anoPublicacao);

        Titulo = titulo.Trim();
        Editora = editora.Trim();
        Edicao = edicao;
        AnoPublicacao = anoPublicacao.Trim();
    }

    private static void ValidateTitulo(string titulo)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new DomainException("Título é obrigatório");
        if (titulo.Length > 40)
            throw new DomainException("Título deve ter no máximo 40 caracteres");
    }

    private static void ValidateEditora(string editora)
    {
        if (string.IsNullOrWhiteSpace(editora))
            throw new DomainException("Editora é obrigatória");
        if (editora.Length > 40)
            throw new DomainException("Editora deve ter no máximo 40 caracteres");
    }

    private static void ValidateEdicao(int edicao)
    {
        if (edicao < 1)
            throw new DomainException("Edição deve ser maior que zero");
    }

    private static void ValidateAnoPublicacao(string anoPublicacao)
    {
        if (string.IsNullOrWhiteSpace(anoPublicacao))
            throw new DomainException("Ano de publicação é obrigatório");
        if (anoPublicacao.Trim().Length != 4)
            throw new DomainException("Ano de publicação deve ter exatamente 4 caracteres");
    }
}