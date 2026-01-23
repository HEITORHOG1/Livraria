namespace Livraria.Domain.Interfaces;

/// <summary>
/// Interface para o padrão Unit of Work.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}