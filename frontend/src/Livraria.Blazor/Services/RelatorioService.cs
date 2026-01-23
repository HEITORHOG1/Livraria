using Livraria.Blazor.Models;
using System.Net.Http.Json;

namespace Livraria.Blazor.Services;

/// <summary>
/// Serviço HTTP para operações de relatórios.
/// </summary>
public class RelatorioService : IRelatorioService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/relatorios";

    public RelatorioService(HttpClient http)
    {
        _http = http;
    }

    public async Task<IEnumerable<RelatorioLivroDto>> GetDadosAsync()
    {
        var response = await _http.GetFromJsonAsync<IEnumerable<RelatorioLivroDto>>($"{BaseUrl}/livros-por-autor");
        return response ?? Enumerable.Empty<RelatorioLivroDto>();
    }

    public async Task<byte[]> DownloadPdfAsync(int[]? autorIds = null)
    {
        var url = $"{BaseUrl}/livros-por-autor/pdf";

        if (autorIds != null && autorIds.Length > 0)
        {
            var queryString = string.Join("&", autorIds.Select(id => $"autorIds={id}"));
            url = $"{url}?{queryString}";
        }

        return await _http.GetByteArrayAsync(url);
    }
}