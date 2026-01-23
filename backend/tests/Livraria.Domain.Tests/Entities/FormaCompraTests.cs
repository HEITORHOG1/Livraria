using FluentAssertions;
using Livraria.Domain.Entities;
using Livraria.Domain.Exceptions;

namespace Livraria.Domain.Tests.Entities;

public class FormaCompraTests
{
    [Fact]
    public void Create_ComDadosValidos_DeveCriarFormaCompra()
    {
        // Act
        var formaCompra = FormaCompra.Create("Balcão");

        // Assert
        formaCompra.Descricao.Should().Be("Balcão");
    }

    [Fact]
    public void Create_ComDescricaoComEspacos_DeveRemoverEspacos()
    {
        // Act
        var formaCompra = FormaCompra.Create("  Balcão  ");

        // Assert
        formaCompra.Descricao.Should().Be("Balcão");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_ComDescricaoVazia_DeveLancarDomainException(string descricao)
    {
        // Act
        var act = () => FormaCompra.Create(descricao);

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
        var act = () => FormaCompra.Create(descricaoLonga);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Descrição deve ter no máximo 20 caracteres");
    }

    [Fact]
    public void Create_ComDescricaoExatamente20Caracteres_DeveCriarFormaCompra()
    {
        // Arrange
        var descricao = new string('A', 20);

        // Act
        var formaCompra = FormaCompra.Create(descricao);

        // Assert
        formaCompra.Descricao.Should().HaveLength(20);
    }
}