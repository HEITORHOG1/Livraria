using Livraria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Livraria.Infrastructure.Data.Seed;

/// <summary>
/// Classe responsável pelo seed de dados iniciais no banco de dados.
/// </summary>
public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(ApplicationDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (await _context.FormasCompra.AnyAsync())
        {
            _logger.LogInformation("Banco já possui dados, seed ignorado");
            return;
        }

        _logger.LogInformation("Iniciando seed de dados...");

        // Formas de Compra (obrigatório para o requisito de preços)
        var formas = new[]
        {
            FormaCompra.Create("Balcão"),
            FormaCompra.Create("Self-Service"),
            FormaCompra.Create("Internet"),
            FormaCompra.Create("Evento")
        };

        await _context.FormasCompra.AddRangeAsync(formas);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seed de dados concluído - {Count} formas de compra criadas", formas.Length);
    }
}