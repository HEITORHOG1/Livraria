using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Livraria.Blazor.Extensions;
using System.Globalization;
using System.Text.RegularExpressions;
using Xunit;

namespace Livraria.Blazor.Tests.Properties;

/// <summary>
/// Property-based tests para formatação de moeda.
/// **Currency Formatting**

/// </summary>
public class CurrencyFormattingPropertyTests
{
    /// <summary>
    /// Para qualquer valor decimal não-negativo, ToMoeda() deve retornar uma string
    /// que começa com "R$" (formato de moeda brasileira).

    /// </summary>
    [Property]
    public bool ToMoeda_ShouldStartWithCurrencySymbol(NonNegativeInt amount)
    {
        var valor = (decimal)amount.Get / 100;
        var resultado = valor.ToMoeda();
        return resultado.StartsWith("R$");
    }

    /// <summary>
    /// Para qualquer valor decimal não-negativo, ToMoeda() deve retornar uma string
    /// com exatamente 2 casas decimais após a vírgula.

    /// </summary>
    [Property]
    public bool ToMoeda_ShouldHaveTwoDecimalPlaces(NonNegativeInt amount)
    {
        var valor = (decimal)amount.Get / 100;
        var resultado = valor.ToMoeda();
        var match = Regex.Match(resultado, @",\d{2}$");
        return match.Success;
    }

    /// <summary>
    /// Para qualquer valor decimal não-negativo, ToMoeda() deve usar vírgula
    /// como separador decimal (padrão brasileiro).

    /// </summary>
    [Property]
    public bool ToMoeda_ShouldUseCommaAsDecimalSeparator(NonNegativeInt amount)
    {
        var valor = (decimal)amount.Get / 100;
        var resultado = valor.ToMoeda();
        return resultado.Contains(",");
    }

    /// <summary>
    /// Para qualquer valor decimal >= 1000, ToMoeda() deve usar ponto
    /// como separador de milhar (padrão brasileiro).

    /// </summary>
    [Property]
    public bool ToMoeda_ShouldUseDotAsThousandSeparator(PositiveInt amount)
    {
        var valor = 1000m + (decimal)amount.Get;
        var resultado = valor.ToMoeda();
        return resultado.Contains(".");
    }

    /// <summary>
    /// Para qualquer valor decimal não-negativo, a formatação deve preservar
    /// o valor numérico quando parseado de volta (considerando arredondamento para 2 casas decimais).

    /// </summary>
    [Property]
    public bool ToMoeda_ShouldPreserveValueOnRoundtrip(NonNegativeInt amount)
    {
        var valor = (decimal)amount.Get / 100;
        var valorArredondado = Math.Round(valor, 2);

        var formatado = valor.ToMoeda();
        var semPrefixo = formatado.Replace("R$", "").Trim();
        var parseado = decimal.Parse(semPrefixo, new CultureInfo("pt-BR"));

        return parseado == valorArredondado;
    }

    /// <summary>
    /// Para qualquer valor decimal não-negativo, ToMoedaInput() não deve conter
    /// o símbolo de moeda "R$".

    /// </summary>
    [Property]
    public bool ToMoedaInput_ShouldNotContainCurrencySymbol(NonNegativeInt amount)
    {
        var valor = (decimal)amount.Get / 100;
        var resultado = valor.ToMoedaInput();
        return !resultado.Contains("R$");
    }

    /// <summary>
    /// Para qualquer valor decimal não-negativo, ToMoedaInput() deve ter
    /// exatamente 2 casas decimais após a vírgula.

    /// </summary>
    [Property]
    public bool ToMoedaInput_ShouldHaveTwoDecimalPlaces(NonNegativeInt amount)
    {
        var valor = (decimal)amount.Get / 100;
        var resultado = valor.ToMoedaInput();
        var match = Regex.Match(resultado, @",\d{2}$");
        return match.Success;
    }

    /// <summary>
    /// Teste específico para valores conhecidos.

    /// </summary>
    [Fact]
    public void ToMoeda_KnownValues_ShouldFormatCorrectly()
    {
        // Arrange & Act & Assert
        0m.ToMoeda().Should().Be("R$ 0,00");
        1m.ToMoeda().Should().Be("R$ 1,00");
        1.5m.ToMoeda().Should().Be("R$ 1,50");
        1234.56m.ToMoeda().Should().Be("R$ 1.234,56");
        1000000m.ToMoeda().Should().Be("R$ 1.000.000,00");
    }

    /// <summary>
    /// Teste específico para valores conhecidos no formato de input.

    /// </summary>
    [Fact]
    public void ToMoedaInput_KnownValues_ShouldFormatCorrectly()
    {
        // Arrange & Act & Assert
        0m.ToMoedaInput().Should().Be("0,00");
        1m.ToMoedaInput().Should().Be("1,00");
        1.5m.ToMoedaInput().Should().Be("1,50");
        1234.56m.ToMoedaInput().Should().Be("1.234,56");
        1000000m.ToMoedaInput().Should().Be("1.000.000,00");
    }
}

