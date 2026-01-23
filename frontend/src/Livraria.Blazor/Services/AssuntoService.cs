using Livraria.Blazor.Models;
using Livraria.Blazor.Models.Requests;
using System.Net.Http.Json;

namespace Livraria.Blazor.Services;

/// <summary>
/// Serviço HTTP para operações de assuntos.
/// </summary>
public class AssuntoService : IAssuntoService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api/assuntos";

    public AssuntoService(HttpClient http)
    {
        _http = http;
    }

    public async Task<IEnumerable<AssuntoDto>> GetAllAsync()
    {
        var response = await _http.GetFromJsonAsync<IEnumerable<AssuntoDto>>(BaseUrl);
        return response ?? Enumerable.Empty<AssuntoDto>();
    }

    public async Task<AssuntoDto?> GetByIdAsync(int codAs)
    {
        return await _http.GetFromJsonAsync<AssuntoDto>($"{BaseUrl}/{codAs}");
    }

    public async Task<ApiResponse<AssuntoDto>> CreateAsync(CreateAssuntoRequest request)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, request);
        return await HandleResponse<AssuntoDto>(response);
    }

    public async Task<ApiResponse<AssuntoDto>> UpdateAsync(int codAs, UpdateAssuntoRequest request)
    {
        var response = await _http.PutAsJsonAsync($"{BaseUrl}/{codAs}", request);
        return await HandleResponse<AssuntoDto>(response);
    }

    public async Task<ApiResponse> DeleteAsync(int codAs)
    {
        var response = await _http.DeleteAsync($"{BaseUrl}/{codAs}");
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