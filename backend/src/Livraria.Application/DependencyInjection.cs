using FluentValidation;
using Livraria.Application.Common.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace Livraria.Application;

/// <summary>
/// Extensões para configuração de injeção de dependência da camada Application.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}