using Livraria.Blazor.Models;

namespace Livraria.Blazor.Services;

/// <summary>
/// Interface para serviço de formas de compra.
/// </summary>
public interface IFormaCompraService
{
    Task<IEnumerable<FormaCompraDto>> GetAllAsync();
}