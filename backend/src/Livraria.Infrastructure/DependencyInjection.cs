using Livraria.Application.Common.Interfaces;
using Livraria.Domain.Interfaces;
using Livraria.Domain.Interfaces.Repositories;
using Livraria.Infrastructure.Data;
using Livraria.Infrastructure.Data.Seed;
using Livraria.Infrastructure.Repositories;
using Livraria.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;

namespace Livraria.Infrastructure;

/// <summary>
/// Extensões para configuração de injeção de dependência da camada Infrastructure.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Only register SQL Server if connection string is provided
        // This allows tests to override with InMemory database
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    connectionString,
                    sqlServer => sqlServer.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
        }

        // Unit of Work
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Repositórios
        services.AddScoped<ILivroRepository, LivroRepository>();
        services.AddScoped<IAutorRepository, AutorRepository>();
        services.AddScoped<IAssuntoRepository, AssuntoRepository>();
        services.AddScoped<IFormaCompraRepository, FormaCompraRepository>();

        // Serviços
        services.AddScoped<IRelatorioService, RelatorioService>();

        // Seed
        services.AddScoped<DatabaseSeeder>();

        // QuestPDF - Licença Community (gratuita)
        QuestPDF.Settings.License = LicenseType.Community;

        return services;
    }
}