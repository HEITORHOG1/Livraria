using Livraria.Blazor.Models;

namespace Livraria.Blazor.Services;

/// <summary>
/// Interface para serviço de relatórios.
/// </summary>
public interface IRelatorioService
{
    Task<IEnumerable<RelatorioLivroDto>> GetDadosAsync();

    Task<byte[]> DownloadPdfAsync(int[]? autorIds = null);
}