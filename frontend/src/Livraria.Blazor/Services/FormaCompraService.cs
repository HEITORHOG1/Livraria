using Livraria.Blazor.Models;
using System.Net.Http.Json;

namespace Livraria.Blazor.Services;

/// <summary>
/// Serviço HTTP para operações de formas de compra.
/// </summary>
public class FormaCompraService : IFormaCompraService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/formas-compra";

    public FormaCompraService(HttpClient http)
    {
        _http = http;
    }

    public async Task<IEnumerable<FormaCompraDto>> GetAllAsync()
    {
        var response = await _http.GetFromJsonAsync<IEnumerable<FormaCompraDto>>(BaseUrl);
        return response ?? Enumerable.Empty<FormaCompraDto>();
    }
}