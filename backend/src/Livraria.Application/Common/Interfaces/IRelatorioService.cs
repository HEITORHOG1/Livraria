using Livraria.Application.DTOs;

namespace Livraria.Application.Common.Interfaces;

/// <summary>
/// Interface para serviço de relatórios.
/// </summary>
public interface IRelatorioService
{
    /// <summary>
    /// Obtém os dados do relatório de livros por autor da VIEW.
    /// </summary>
    Task<IEnumerable<RelatorioLivroDto>> GetDadosRelatorioAsync(CancellationToken ct = default);

    /// <summary>
    /// Gera o PDF do relatório de livros por autor.
    /// </summary>
    /// <param name="autorIds">IDs dos autores para filtrar (opcional). Se null, inclui todos.</param>
    Task<byte[]> GerarPdfAsync(int[]? autorIds = null, CancellationToken ct = default);
}