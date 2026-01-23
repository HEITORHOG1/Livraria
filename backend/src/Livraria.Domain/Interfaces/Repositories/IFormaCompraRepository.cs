using Livraria.Domain.Entities;

namespace Livraria.Domain.Interfaces.Repositories;

/// <summary>
/// Interface do repositório de Formas de Compra.
/// </summary>
public interface IFormaCompraRepository
{
    Task<IEnumerable<FormaCompra>> GetAllAsync(CancellationToken ct = default);

    Task<bool> ExistemAsync(IEnumerable<int> codFcs, CancellationToken ct = default);
}