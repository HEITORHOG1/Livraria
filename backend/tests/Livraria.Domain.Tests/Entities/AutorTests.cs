using FluentAssertions;
using Livraria.Domain.Entities;
using Livraria.Domain.Exceptions;

namespace Livraria.Domain.Tests.Entities;

public class AutorTests
{
    [Fact]
    public void Create_ComDadosValidos_DeveCriarAutor()
    {
        // Act
        var autor = Autor.Create("Robert C. Martin");

        // Assert
        autor.Nome.Should().Be("Robert C. Martin");
    }

    [Fact]
    public void Create_ComNomeComEspacos_DeveRemoverEspacos()
    {
        // Act
        var autor = Autor.Create("  Robert C. Martin  ");

        // Assert
        autor.Nome.Should().Be("Robert C. Martin");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_ComNomeVazio_DeveLancarDomainException(string nome)
    {
        // Act
        var act = () => Autor.Create(nome);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Nome é obrigatório");
    }

    [Fact]
    public void Create_ComNomeMaiorQue40Caracteres_DeveLancarDomainException()
    {
        // Arrange
        var nomeLongo = new string('A', 41);

        // Act
        var act = () => Autor.Create(nomeLongo);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Nome deve ter no máximo 40 caracteres");
    }

    [Fact]
    public void Create_ComNomeExatamente40Caracteres_DeveCriarAutor()
    {
        // Arrange
        var nome = new string('A', 40);

        // Act
        var autor = Autor.Create(nome);

        // Assert
        autor.Nome.Should().HaveLength(40);
    }

    [Fact]
    public void Update_ComDadosValidos_DeveAtualizarAutor()
    {
        // Arrange
        var autor = Autor.Create("Nome Original");

        // Act
        autor.Update("Novo Nome");

        // Assert
        autor.Nome.Should().Be("Novo Nome");
    }

    [Fact]
    public void Update_ComNomeInvalido_DeveLancarDomainException()
    {
        // Arrange
        var autor = Autor.Create("Nome Original");

        // Act
        var act = () => autor.Update("");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Nome é obrigatório");
    }

    [Fact]
    public void Update_ComNomeMaiorQue40Caracteres_DeveLancarDomainException()
    {
        // Arrange
        var autor = Autor.Create("Nome Original");
        var nomeLongo = new string('A', 41);

        // Act
        var act = () => autor.Update(nomeLongo);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Nome deve ter no máximo 40 caracteres");
    }
}