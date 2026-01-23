using Livraria.Domain.Entities;

namespace Livraria.Domain.Interfaces.Repositories;

/// <summary>
/// Interface do repositório de Assuntos.
/// </summary>
public interface IAssuntoRepository
{
    Task<Assunto?> GetByIdAsync(int codAs, CancellationToken ct = default);

    Task<IEnumerable<Assunto>> GetAllAsync(CancellationToken ct = default);

    Task<bool> ExistemAsync(IEnumerable<int> codAss, CancellationToken ct = default);

    Task AddAsync(Assunto assunto, CancellationToken ct = default);

    void Update(Assunto assunto);

    void Delete(Assunto assunto);
}