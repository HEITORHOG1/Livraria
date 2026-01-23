using Livraria.Domain.Entities;

namespace Livraria.Domain.Interfaces.Repositories;

/// <summary>
/// Interface do repositório de Livros.
/// </summary>
public interface ILivroRepository
{
    Task<Livro?> GetByIdAsync(int codL, CancellationToken ct = default);

    Task<Livro?> GetByIdWithRelationsAsync(int codL, CancellationToken ct = default);

    Task<IEnumerable<Livro>> GetAllAsync(CancellationToken ct = default);

    Task<IEnumerable<Livro>> GetAllWithRelationsAsync(CancellationToken ct = default);

    Task AddAsync(Livro livro, CancellationToken ct = default);

    void Update(Livro livro);

    void Delete(Livro livro);
}