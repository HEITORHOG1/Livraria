using Livraria.Domain.Entities;

namespace Livraria.Domain.Interfaces.Repositories;

/// <summary>
/// Interface do repositório de Autores.
/// </summary>
public interface IAutorRepository
{
    Task<Autor?> GetByIdAsync(int codAu, CancellationToken ct = default);

    Task<IEnumerable<Autor>> GetAllAsync(CancellationToken ct = default);

    Task<bool> ExistemAsync(IEnumerable<int> codAus, CancellationToken ct = default);

    Task AddAsync(Autor autor, CancellationToken ct = default);

    void Update(Autor autor);

    void Delete(Autor autor);
}