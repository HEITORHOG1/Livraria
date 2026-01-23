using FsCheck;
using FsCheck.Xunit;
using Livraria.Domain.Entities;
using Livraria.Domain.Exceptions;

namespace Livraria.Domain.Tests.Properties;

/// <summary>
/// Invalid Entity Validation Rejection

///
/// For any invalid input data, the system should reject entity creation with appropriate validation errors.
/// </summary>
public class EntityValidationPropertyTests
{
    #region Livro Properties

    /// <summary>
    /// For any string longer than 40 characters, Livro.Create should reject with validation error.

    /// </summary>
    [Property]
    public bool Livro_TituloOver40_ShouldReject(PositiveInt extraLength)
    {
        var titulo = new string('a', 41 + (extraLength.Get % 60));
        try
        {
            Livro.Create(titulo, "Editora", 1, "2024");
            return false;
        }
        catch (DomainException ex)
        {
            return ex.Message == "Título deve ter no máximo 40 caracteres";
        }
    }

    /// <summary>
    /// For any string longer than 40 characters, Livro.Create should reject with validation error.

    /// </summary>
    [Property]
    public bool Livro_EditoraOver40_ShouldReject(PositiveInt extraLength)
    {
        var editora = new string('a', 41 + (extraLength.Get % 60));
        try
        {
            Livro.Create("Titulo", editora, 1, "2024");
            return false;
        }
        catch (DomainException ex)
        {
            return ex.Message == "Editora deve ter no máximo 40 caracteres";
        }
    }

    /// <summary>
    /// For any integer less than 1, Livro.Create should reject with validation error.

    /// </summary>
    [Property]
    public bool Livro_EdicaoLessThan1_ShouldReject(NonNegativeInt nonNegative)
    {
        var edicao = -nonNegative.Get; // Make it 0 or negative
        try
        {
            Livro.Create("Titulo", "Editora", edicao, "2024");
            return false;
        }
        catch (DomainException ex)
        {
            return ex.Message == "Edição deve ser maior que zero";
        }
    }

    /// <summary>
    /// For any string not exactly 4 characters (shorter), Livro.Create should reject.

    /// </summary>
    [Property]
    public bool Livro_AnoPublicacaoTooShort_ShouldReject(PositiveInt length)
    {
        var anoLength = (length.Get % 3) + 1; // 1, 2, or 3 characters
        var ano = new string('2', anoLength);
        try
        {
            Livro.Create("Titulo", "Editora", 1, ano);
            return false;
        }
        catch (DomainException ex)
        {
            return ex.Message == "Ano de publicação deve ter exatamente 4 caracteres";
        }
    }

    /// <summary>
    /// For any string not exactly 4 characters (longer), Livro.Create should reject.

    /// </summary>
    [Property]
    public bool Livro_AnoPublicacaoTooLong_ShouldReject(PositiveInt extraLength)
    {
        var ano = new string('2', 5 + (extraLength.Get % 10)); // 5+ characters
        try
        {
            Livro.Create("Titulo", "Editora", 1, ano);
            return false;
        }
        catch (DomainException ex)
        {
            return ex.Message == "Ano de publicação deve ter exatamente 4 caracteres";
        }
    }

    /// <summary>
    /// For any valid Livro data, creation should succeed and preserve the data.

    /// </summary>
    [Property]
    public bool Livro_ValidData_ShouldCreate(PositiveInt tituloLen, PositiveInt editoraLen, PositiveInt edicao, PositiveInt ano)
    {
        var titulo = new string('a', (tituloLen.Get % 40) + 1);
        var editora = new string('b', (editoraLen.Get % 40) + 1);
        var edicaoVal = edicao.Get;
        var anoVal = ((ano.Get % 9000) + 1000).ToString(); // 1000-9999

        var livro = Livro.Create(titulo, editora, edicaoVal, anoVal);

        return livro.Titulo == titulo
            && livro.Editora == editora
            && livro.Edicao == edicaoVal
            && livro.AnoPublicacao == anoVal;
    }

    #endregion Livro Properties

    #region Autor Properties

    /// <summary>
    /// For any string longer than 40 characters, Autor.Create should reject with validation error.

    /// </summary>
    [Property]
    public bool Autor_NomeOver40_ShouldReject(PositiveInt extraLength)
    {
        var nome = new string('a', 41 + (extraLength.Get % 60));
        try
        {
            Autor.Create(nome);
            return false;
        }
        catch (DomainException ex)
        {
            return ex.Message == "Nome deve ter no máximo 40 caracteres";
        }
    }

    /// <summary>
    /// For any valid Autor data, creation should succeed and preserve the data.

    /// </summary>
    [Property]
    public bool Autor_ValidData_ShouldCreate(PositiveInt nomeLen)
    {
        var nome = new string('a', (nomeLen.Get % 40) + 1);
        var autor = Autor.Create(nome);
        return autor.Nome == nome;
    }

    #endregion Autor Properties

    #region Assunto Properties

    /// <summary>
    /// For any string longer than 20 characters, Assunto.Create should reject with validation error.

    /// </summary>
    [Property]
    public bool Assunto_DescricaoOver20_ShouldReject(PositiveInt extraLength)
    {
        var descricao = new string('a', 21 + (extraLength.Get % 30));
        try
        {
            Assunto.Create(descricao);
            return false;
        }
        catch (DomainException ex)
        {
            return ex.Message == "Descrição deve ter no máximo 20 caracteres";
        }
    }

    /// <summary>
    /// For any valid Assunto data, creation should succeed and preserve the data.

    /// </summary>
    [Property]
    public bool Assunto_ValidData_ShouldCreate(PositiveInt descricaoLen)
    {
        var descricao = new string('a', (descricaoLen.Get % 20) + 1);
        var assunto = Assunto.Create(descricao);
        return assunto.Descricao == descricao;
    }

    #endregion Assunto Properties

    #region LivroPreco Properties

    /// <summary>
    /// For any negative decimal value, LivroPreco.ValidateValor should reject with validation error.

    /// </summary>
    [Property]
    public bool LivroPreco_NegativeValor_ShouldReject(PositiveInt amount)
    {
        var valor = -(decimal)(amount.Get + 1) / 100; // Always negative
        try
        {
            LivroPreco.ValidateValor(valor);
            return false;
        }
        catch (DomainException ex)
        {
            return ex.Message == "Valor não pode ser negativo";
        }
    }

    /// <summary>
    /// For any non-negative decimal value, LivroPreco.ValidateValor should succeed.

    /// </summary>
    [Property]
    public bool LivroPreco_NonNegativeValor_ShouldAccept(NonNegativeInt amount)
    {
        var valor = (decimal)amount.Get / 100;
        try
        {
            LivroPreco.ValidateValor(valor);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion LivroPreco Properties
}

