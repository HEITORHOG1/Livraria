using Livraria.Domain.Entities;
using Livraria.Domain.Interfaces.Repositories;
using Livraria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Livraria.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de Assuntos.
/// </summary>
public class AssuntoRepository : IAssuntoRepository
{
    private readonly ApplicationDbContext _context;

    public AssuntoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Assunto?> GetByIdAsync(int codAs, CancellationToken ct = default)
    {
        return await _context.Assuntos.FindAsync([codAs], ct);
    }

    public async Task<IEnumerable<Assunto>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Assuntos.ToListAsync(ct);
    }

    public async Task<bool> ExistemAsync(IEnumerable<int> codAss, CancellationToken ct = default)
    {
        var codAssList = codAss.ToList();
        if (codAssList.Count == 0)
            return true;

        var count = await _context.Assuntos
            .Where(a => codAssList.Contains(a.CodAs))
            .CountAsync(ct);

        return count == codAssList.Count;
    }

    public async Task AddAsync(Assunto assunto, CancellationToken ct = default)
    {
        await _context.Assuntos.AddAsync(assunto, ct);
    }

    public void Update(Assunto assunto)
    {
        _context.Assuntos.Update(assunto);
    }

    public void Delete(Assunto assunto)
    {
        _context.Assuntos.Remove(assunto);
    }
}