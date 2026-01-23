using Livraria.Blazor.Models;
using Livraria.Blazor.Models.Requests;
using System.Net.Http.Json;

namespace Livraria.Blazor.Services;

/// <summary>
/// Serviço HTTP para operações de livros.
/// </summary>
public class LivroService : ILivroService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/livros";

    public LivroService(HttpClient http)
    {
        _http = http;
    }

    public async Task<IEnumerable<LivroListDto>> GetAllAsync()
    {
        var response = await _http.GetFromJsonAsync<IEnumerable<LivroListDto>>(BaseUrl);
        return response ?? Enumerable.Empty<LivroListDto>();
    }

    public async Task<LivroDto?> GetByIdAsync(int codL)
    {
        return await _http.GetFromJsonAsync<LivroDto>($"{BaseUrl}/{codL}");
    }

    public async Task<ApiResponse<LivroDto>> CreateAsync(CreateLivroRequest request)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, request);
        return await HandleResponse<LivroDto>(response);
    }

    public async Task<ApiResponse<LivroDto>> UpdateAsync(int codL, UpdateLivroRequest request)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{codL}", request);
        return await HandleResponse<LivroDto>(response);
    }

    public async Task<ApiResponse> DeleteAsync(int codL)
    {
        var response = await _http.DeleteAsync($"{BaseUrl}/{codL}");
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