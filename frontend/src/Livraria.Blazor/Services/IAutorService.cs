using Livraria.Blazor.Models;
using Livraria.Blazor.Models.Requests;

namespace Livraria.Blazor.Services;

/// <summary>
/// Interface para serviço de autores.
/// </summary>
public interface IAutorService
{
    Task<IEnumerable<AutorDto>> GetAllAsync();

    Task<AutorDto?> GetByIdAsync(int codAu);

    Task<ApiResponse<AutorDto>> CreateAsync(CreateAutorRequest request);

    Task<ApiResponse<AutorDto>> UpdateAsync(int codAu, UpdateAutorRequest request);

    Task<ApiResponse> DeleteAsync(int codAu);
}