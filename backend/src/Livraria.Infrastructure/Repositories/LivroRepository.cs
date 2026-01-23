using Livraria.Domain.Entities;
using Livraria.Domain.Interfaces.Repositories;
using Livraria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Livraria.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de Livros.
/// </summary>
public class LivroRepository : ILivroRepository
{
    private readonly ApplicationDbContext _context;

    public LivroRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Livro?> GetByIdAsync(int codL, CancellationToken ct = default)
    {
        return await _context.Livros.FindAsync([codL], ct);
    }

    public async Task<Livro?> GetByIdWithRelationsAsync(int codL, CancellationToken ct = default)
    {
        return await _context.Livros
            .Include(l => l.LivroAutores)
                .ThenInclude(la => la.Autor)
            .Include(l => l.LivroAssuntos)
                .ThenInclude(la => la.Assunto)
            .Include(l => l.LivroPrecos)
                .ThenInclude(lp => lp.FormaCompra)
            .FirstOrDefaultAsync(l => l.CodL == codL, ct);
    }

    public async Task<IEnumerable<Livro>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Livros.ToListAsync(ct);
    }

    public async Task<IEnumerable<Livro>> GetAllWithRelationsAsync(CancellationToken ct = default)
    {
        return await _context.Livros
            .Include(l => l.LivroAutores)
                .ThenInclude(la => la.Autor)
            .Include(l => l.LivroAssuntos)
                .ThenInclude(la => la.Assunto)
            .Include(l => l.LivroPrecos)
                .ThenInclude(lp => lp.FormaCompra)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Livro livro, CancellationToken ct = default)
    {
        await _context.Livros.AddAsync(livro, ct);
    }

    public void Update(Livro livro)
    {
        _context.Livros.Update(livro);
    }

    public void Delete(Livro livro)
    {
        _context.Livros.Remove(livro);
    }
}