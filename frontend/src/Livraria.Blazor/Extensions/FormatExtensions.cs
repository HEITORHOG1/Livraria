using System.Globalization;

namespace Livraria.Blazor.Extensions;

/// <summary>
/// Extensões para formatação de valores.
/// </summary>
public static class FormatExtensions
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    /// <summary>
    /// Formata valor como moeda brasileira: R$ 1.234,56
    /// </summary>
    public static string ToMoeda(this decimal valor)
        => valor.ToString("C2", PtBr);

    /// <summary>
    /// Formata valor para input numérico: 1.234,56
    /// </summary>
    public static string ToMoedaInput(this decimal valor)
        => valor.ToString("N2", PtBr);
}