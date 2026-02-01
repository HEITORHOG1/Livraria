using Livraria.Domain.Entities;
using Livraria.Domain.Interfaces.Repositories;
using Livraria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Livraria.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de Autores.
/// </summary>
public class AutorRepository : IAutorRepository
{
    private readonly ApplicationDbContext _context;

    public AutorRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Autor?> GetByIdAsync(int codAu, CancellationToken ct = default)
    {
        return await _context.Autores.FindAsync([codAu], ct);
    }

    public async Task<IEnumerable<Autor>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Autores.ToListAsync(ct);
    }

    public async Task<bool> ExistemAsync(IEnumerable<int> codAus, CancellationToken ct = default)
    {
        var codAusList = codAus.ToList();
        if (codAusList.Count == 0)
            return true;

        var count = await _context.Autores
            .Where(a => codAusList.Contains(a.CodAu))
            .CountAsync(ct);

        return count == codAusList.Count;
    }

    public async Task AddAsync(Autor autor, CancellationToken ct = default)
    {
        await _context.Autores.AddAsync(autor, ct);
    }

    public void Update(Autor autor)
    {
        _context.Autores.Update(autor);
    }

    public void Delete(Autor autor)
    {
        _context.Autores.Remove(autor);
    }

    public async Task<bool> HasLivrosVinculadosAsync(int codAu, CancellationToken ct = default)
    {
        return await _context.Set<LivroAutor>()
            .AnyAsync(la => la.Autor_CodAu == codAu, ct);
    }
}