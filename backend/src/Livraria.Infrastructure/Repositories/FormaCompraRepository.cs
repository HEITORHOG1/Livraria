using Livraria.Domain.Entities;
using Livraria.Domain.Interfaces.Repositories;
using Livraria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Livraria.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de Formas de Compra.
/// </summary>
public class FormaCompraRepository : IFormaCompraRepository
{
    private readonly ApplicationDbContext _context;

    public FormaCompraRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FormaCompra>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.FormasCompra.ToListAsync(ct);
    }

    public async Task<bool> ExistemAsync(IEnumerable<int> codFcs, CancellationToken ct = default)
    {
        var codFcsList = codFcs.ToList();
        if (codFcsList.Count == 0)
            return true;

        var count = await _context.FormasCompra
            .Where(f => codFcsList.Contains(f.CodFc))
            .CountAsync(ct);

        return count == codFcsList.Count;
    }
}