using Livraria.Blazor.Models;
using Livraria.Blazor.Models.Requests;
using System.Net.Http.Json;

namespace Livraria.Blazor.Services;

/// <summary>
/// Serviço HTTP para operações de autores.
/// </summary>
public class AutorService : IAutorService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/autores";

    public AutorService(HttpClient http)
    {
        _http = http;
    }

    public async Task<IEnumerable<AutorDto>> GetAllAsync()
    {
        var response = await _http.GetFromJsonAsync<IEnumerable<AutorDto>>(BaseUrl);
        return response ?? Enumerable.Empty<AutorDto>();
    }

    public async Task<AutorDto?> GetByIdAsync(int codAu)
    {
        return await _http.GetFromJsonAsync<AutorDto>($"{BaseUrl}/{codAu}");
    }

    public async Task<ApiResponse<AutorDto>> CreateAsync(CreateAutorRequest request)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, request);
        return await HandleResponse<AutorDto>(response);
    }

    public async Task<ApiResponse<AutorDto>> UpdateAsync(int codAu, UpdateAutorRequest request)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{codAu}", request);
        return await HandleResponse<AutorDto>(response);
    }

    public async Task<ApiResponse> DeleteAsync(int codAu)
    {
        var response = await _http.DeleteAsync($"{BaseUrl}/{codAu}");
        return await HandleResponse(response);
    }

    private static async Task<ApiResponse<T>> HandleResponse<T>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<T>();
            return ApiResponse<T>.Success(data!);
        }

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return ApiResponse<T>.Failure(error?.Message ?? "Erro desconhecido");
    }

    private static async Task<ApiResponse> HandleResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return ApiResponse.Success();
        }

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return ApiResponse.Failure(error?.Message ?? "Erro desconhecido");
    }
}