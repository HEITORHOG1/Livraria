using Livraria.API.Middleware;
using Livraria.Application;
using Livraria.Infrastructure;
using Livraria.Infrastructure.Data;
using Livraria.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Controllers
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Livraria API",
        Version = "v1",
        Description = "API para gerenciamento de livros, autores e assuntos"
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.SetIsOriginAllowed(_ => true) // Allow any origin in development
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Exception handling middleware (must be first)
app.UseExceptionHandling();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Livraria API v1");
    });
}

// Apply migrations and seed data
if (builder.Configuration.GetValue<bool>("RunMigrations"))
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Aplicando migrations...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Migrations aplicadas com sucesso");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erro ao aplicar migrations");
    }
}

if (builder.Configuration.GetValue<bool>("SeedData"))
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

    try
    {
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro ao executar seed de dados");
    }
}

app.UseHttpsRedirection();
app.UseCors("AllowBlazor");
app.MapControllers();

app.Run();

// Make Program class accessible for integration tests
public partial class Program
{ }