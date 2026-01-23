using FluentAssertions;
using Livraria.Domain.Entities;
using Livraria.Domain.Exceptions;

namespace Livraria.Domain.Tests.Entities;

public class LivroPrecoTests
{
    [Fact]
    public void ValidateValor_ComValorPositivo_NaoDeveLancarExcecao()
    {
        // Act
        var act = () => LivroPreco.ValidateValor(100.50m);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateValor_ComValorZero_NaoDeveLancarExcecao()
    {
        // Act
        var act = () => LivroPreco.ValidateValor(0m);

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void ValidateValor_ComValorNegativo_DeveLancarDomainException(decimal valor)
    {
        // Act
        var act = () => LivroPreco.ValidateValor(valor);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Valor não pode ser negativo");
    }
}