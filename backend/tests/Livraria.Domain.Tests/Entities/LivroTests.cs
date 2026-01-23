using FluentAssertions;
using Livraria.Domain.Entities;
using Livraria.Domain.Exceptions;

namespace Livraria.Domain.Tests.Entities;

public class LivroTests
{
    [Fact]
    public void Create_ComDadosValidos_DeveCriarLivro()
    {
        // Act
        var livro = Livro.Create("Clean Code", "Alta Books", 1, "2009");

        // Assert
        livro.Titulo.Should().Be("Clean Code");
        livro.Editora.Should().Be("Alta Books");
        livro.Edicao.Should().Be(1);
        livro.AnoPublicacao.Should().Be("2009");
    }

    [Fact]
    public void Create_ComTituloComEspacos_DeveRemoverEspacos()
    {
        // Act
        var livro = Livro.Create("  Clean Code  ", "Alta Books", 1, "2009");

        // Assert
        livro.Titulo.Should().Be("Clean Code");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_ComTituloVazio_DeveLancarDomainException(string titulo)
    {
        // Act
        var act = () => Livro.Create(titulo, "Editora", 1, "2024");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Título é obrigatório");
    }

    [Fact]
    public void Create_ComTituloMaiorQue40Caracteres_DeveLancarDomainException()
    {
        // Arrange
        var tituloLongo = new string('A', 41);

        // Act
        var act = () => Livro.Create(tituloLongo, "Editora", 1, "2024");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Título deve ter no máximo 40 caracteres");
    }

    [Fact]
    public void Create_ComTituloExatamente40Caracteres_DeveCriarLivro()
    {
        // Arrange
        var titulo = new string('A', 40);

        // Act
        var livro = Livro.Create(titulo, "Editora", 1, "2024");

        // Assert
        livro.Titulo.Should().HaveLength(40);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_ComEditoraVazia_DeveLancarDomainException(string editora)
    {
        // Act
        var act = () => Livro.Create("Titulo", editora, 1, "2024");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Editora é obrigatória");
    }

    [Fact]
    public void Create_ComEditoraMaiorQue40Caracteres_DeveLancarDomainException()
    {
        // Arrange
        var editoraLonga = new string('A', 41);

        // Act
        var act = () => Livro.Create("Titulo", editoraLonga, 1, "2024");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Editora deve ter no máximo 40 caracteres");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_ComEdicaoMenorQue1_DeveLancarDomainException(int edicao)
    {
        // Act
        var act = () => Livro.Create("Titulo", "Editora", edicao, "2024");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Edição deve ser maior que zero");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_ComAnoPublicacaoVazio_DeveLancarDomainException(string ano)
    {
        // Act
        var act = () => Livro.Create("Titulo", "Editora", 1, ano);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Ano de publicação é obrigatório");
    }

    [Theory]
    [InlineData("202")]
    [InlineData("20245")]
    [InlineData("24")]
    public void Create_ComAnoPublicacaoDiferenteDe4Caracteres_DeveLancarDomainException(string ano)
    {
        // Act
        var act = () => Livro.Create("Titulo", "Editora", 1, ano);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Ano de publicação deve ter exatamente 4 caracteres");
    }

    [Fact]
    public void Update_ComDadosValidos_DeveAtualizarLivro()
    {
        // Arrange
        var livro = Livro.Create("Titulo Original", "Editora Original", 1, "2020");

        // Act
        livro.Update("Novo Titulo", "Nova Editora", 2, "2024");

        // Assert
        livro.Titulo.Should().Be("Novo Titulo");
        livro.Editora.Should().Be("Nova Editora");
        livro.Edicao.Should().Be(2);
        livro.AnoPublicacao.Should().Be("2024");
    }

    [Fact]
    public void Update_ComTituloInvalido_DeveLancarDomainException()
    {
        // Arrange
        var livro = Livro.Create("Titulo", "Editora", 1, "2024");

        // Act
        var act = () => livro.Update("", "Editora", 1, "2024");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Título é obrigatório");
    }
}