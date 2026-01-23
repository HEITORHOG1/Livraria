using Livraria.Blazor.Models;
using Livraria.Blazor.Models.Requests;

namespace Livraria.Blazor.Services;

/// <summary>
/// Interface para serviço de assuntos.
/// </summary>
public interface IAssuntoService
{
    Task<IEnumerable<AssuntoDto>> GetAllAsync();

    Task<AssuntoDto?> GetByIdAsync(int codAs);

    Task<ApiResponse<AssuntoDto>> CreateAsync(CreateAssuntoRequest request);

    Task<ApiResponse<AssuntoDto>> UpdateAsync(int codAs, UpdateAssuntoRequest request);

    Task<ApiResponse> DeleteAsync(int codAs);
}