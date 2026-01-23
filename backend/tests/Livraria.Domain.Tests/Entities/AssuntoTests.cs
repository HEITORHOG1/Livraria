using FluentAssertions;
using Livraria.Domain.Entities;
using Livraria.Domain.Exceptions;

namespace Livraria.Domain.Tests.Entities;

public class AssuntoTests
{
    [Fact]
    public void Create_ComDadosValidos_DeveCriarAssunto()
    {
        // Act
        var assunto = Assunto.Create("Programação");

        // Assert
        assunto.Descricao.Should().Be("Programação");
    }

    [Fact]
    public void Create_ComDescricaoComEspacos_DeveRemoverEspacos()
    {
        // Act
        var assunto = Assunto.Create("  Programação  ");

        // Assert
        assunto.Descricao.Should().Be("Programação");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_ComDescricaoVazia_DeveLancarDomainException(string descricao)
    {
        // Act
        var act = () => Assunto.Create(descricao);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Descrição é obrigatória");
    }

    [Fact]
    public void Create_ComDescricaoMaiorQue20Caracteres_DeveLancarDomainException()
    {
        // Arrange
        var descricaoLonga = new string('A', 21);

        // Act
        var act = () => Assunto.Create(descricaoLonga);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Descrição deve ter no máximo 20 caracteres");
    }

    [Fact]
    public void Create_ComDescricaoExatamente20Caracteres_DeveCriarAssunto()
    {
        // Arrange
        var descricao = new string('A', 20);

        // Act
        var assunto = Assunto.Create(descricao);

        // Assert
        assunto.Descricao.Should().HaveLength(20);
    }

    [Fact]
    public void Update_ComDadosValidos_DeveAtualizarAssunto()
    {
        // Arrange
        var assunto = Assunto.Create("Original");

        // Act
        assunto.Update("Atualizado");

        // Assert
        assunto.Descricao.Should().Be("Atualizado");
    }

    [Fact]
    public void Update_ComDescricaoInvalida_DeveLancarDomainException()
    {
        // Arrange
        var assunto = Assunto.Create("Original");

        // Act
        var act = () => assunto.Update("");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Descrição é obrigatória");
    }

    [Fact]
    public void Update_ComDescricaoMaiorQue20Caracteres_DeveLancarDomainException()
    {
        // Arrange
        var assunto = Assunto.Create("Original");
        var descricaoLonga = new string('A', 21);

        // Act
        var act = () => assunto.Update(descricaoLonga);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Descrição deve ter no máximo 20 caracteres");
    }
}