using Livraria.Blazor.Models;
using Livraria.Blazor.Models.Requests;

namespace Livraria.Blazor.Services;

/// <summary>
/// Interface para serviço de livros.
/// </summary>
public interface ILivroService
{
    Task<IEnumerable<LivroListDto>> GetAllAsync();

    Task<LivroDto?> GetByIdAsync(int codL);

    Task<ApiResponse<LivroDto>> CreateAsync(CreateLivroRequest request);

    Task<ApiResponse<LivroDto>> UpdateAsync(int codL, UpdateLivroRequest request);

    Task<ApiResponse> DeleteAsync(int codL);
}