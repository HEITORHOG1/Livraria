using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Livraria.Application.Assuntos.Commands.CreateAssunto;
using Livraria.Application.Autores.Commands.CreateAutor;
using Livraria.Application.Livros.Commands.CreateLivro;
using Livraria.Domain.Interfaces;
using Livraria.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Livraria.Integration.Tests.Properties;

/// <summary>
/// Property 16: Domain Validation Error Messages

///
/// For any domain validation failure (empty required field, field exceeding max length, invalid value),
/// the system should return a specific error message describing the exact validation failure.
/// </summary>
public class ErrorHandlingPropertyTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private static readonly string DbName = $"TestDb_{Guid.NewGuid()}";

    public ErrorHandlingPropertyTests()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");

                // Disable migrations and seed for tests
                builder.UseSetting("RunMigrations", "false");
                builder.UseSetting("SeedData", "false");
                builder.UseSetting("ConnectionStrings:DefaultConnection", "");

                builder.ConfigureServices(services =>
                {
                    // Find and remove the existing DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Remove IUnitOfWork registration
                    var unitOfWorkDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IUnitOfWork));
                    if (unitOfWorkDescriptor != null)
                    {
                        services.Remove(unitOfWorkDescriptor);
                    }

                    // Remove ApplicationDbContext registration
                    var dbContextDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(ApplicationDbContext));
                    if (dbContextDescriptor != null)
                    {
                        services.Remove(dbContextDescriptor);
                    }

                    // Add in-memory database
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(DbName);
                    });

                    // Re-register IUnitOfWork
                    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
                });
            });

        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Property: For any Livro with Titulo exceeding 40 characters, the API should return 400 with specific error message.

    /// </summary>
    [Property(MaxTest = 50)]
    public void Livro_TituloOver40_ShouldReturnSpecificErrorMessage(PositiveInt extraLength)
    {
        var titulo = new string('a', 41 + (extraLength.Get % 60));
        var command = new CreateLivroCommand(
            titulo, "Editora", 1, "2024",
            [1], [], new Dictionary<int, decimal>());

        var response = _client.PostAsJsonAsync("/api/livros", command).Result;

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = response.Content.ReadAsStringAsync().Result;
        content.Should().Contain("40 caracteres");
    }

    /// <summary>
    /// Property: For any Livro with empty Titulo, the API should return 400 with specific error message.

    /// </summary>
    [Fact]
    public async Task Livro_EmptyTitulo_ShouldReturnSpecificErrorMessage()
    {
        var command = new CreateLivroCommand(
            "", "Editora", 1, "2024",
            [1], [], new Dictionary<int, decimal>());

        var response = await _client.PostAsJsonAsync("/api/livros", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("obrigat");
    }

    /// <summary>
    /// Property: For any Livro with Edicao less than 1, the API should return 400 with specific error message.

    /// </summary>
    [Property(MaxTest = 50)]
    public void Livro_EdicaoLessThan1_ShouldReturnSpecificErrorMessage(NonNegativeInt nonNegative)
    {
        var edicao = -nonNegative.Get; // 0 or negative
        var command = new CreateLivroCommand(
            "Titulo", "Editora", edicao, "2024",
            [1], [], new Dictionary<int, decimal>());

        var response = _client.PostAsJsonAsync("/api/livros", command).Result;

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = response.Content.ReadAsStringAsync().Result;
        content.Should().Contain("maior que zero");
    }

    /// <summary>
    /// Property: For any Livro with AnoPublicacao not exactly 4 characters, the API should return 400 with specific error message.

    /// </summary>
    [Property(MaxTest = 50)]
    public void Livro_AnoPublicacaoInvalid_ShouldReturnSpecificErrorMessage(PositiveInt length)
    {
        // Generate string with length != 4 (1-3 or 5+)
        var anoLength = length.Get % 10;
        if (anoLength == 4) anoLength = 5;
        if (anoLength == 0) anoLength = 1;
        var ano = new string('2', anoLength);

        var command = new CreateLivroCommand(
            "Titulo", "Editora", 1, ano,
            [1], [], new Dictionary<int, decimal>());

        var response = _client.PostAsJsonAsync("/api/livros", command).Result;

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = response.Content.ReadAsStringAsync().Result;
        content.Should().Contain("4 caracteres");
    }

    /// <summary>
    /// Property: For any Autor with Nome exceeding 40 characters, the API should return 400 with specific error message.

    /// </summary>
    [Property(MaxTest = 50)]
    public void Autor_NomeOver40_ShouldReturnSpecificErrorMessage(PositiveInt extraLength)
    {
        var nome = new string('a', 41 + (extraLength.Get % 60));
        var command = new CreateAutorCommand(nome);

        var response = _client.PostAsJsonAsync("/api/autores", command).Result;

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = response.Content.ReadAsStringAsync().Result;
        content.Should().Contain("40 caracteres");
    }

    /// <summary>
    /// Property: For any Autor with empty Nome, the API should return 400 with specific error message.

    /// </summary>
    [Fact]
    public async Task Autor_EmptyNome_ShouldReturnSpecificErrorMessage()
    {
        var command = new CreateAutorCommand("");

        var response = await _client.PostAsJsonAsync("/api/autores", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("obrigat");
    }

    /// <summary>
    /// Property: For any Assunto with Descricao exceeding 20 characters, the API should return 400 with specific error message.

    /// </summary>
    [Property(MaxTest = 50)]
    public void Assunto_DescricaoOver20_ShouldReturnSpecificErrorMessage(PositiveInt extraLength)
    {
        var descricao = new string('a', 21 + (extraLength.Get % 30));
        var command = new CreateAssuntoCommand(descricao);

        var response = _client.PostAsJsonAsync("/api/assuntos", command).Result;

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = response.Content.ReadAsStringAsync().Result;
        content.Should().Contain("20 caracteres");
    }

    /// <summary>
    /// Property: For any Assunto with empty Descricao, the API should return 400 with specific error message.

    /// </summary>
    [Fact]
    public async Task Assunto_EmptyDescricao_ShouldReturnSpecificErrorMessage()
    {
        var command = new CreateAssuntoCommand("");

        var response = await _client.PostAsJsonAsync("/api/assuntos", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("obrigat");
    }

    /// <summary>
    /// Property: For any request to get a non-existent Livro, the API should return 404 with specific error message.

    /// </summary>
    [Property(MaxTest = 50)]
    public void Livro_NotFound_ShouldReturnSpecificErrorMessage(PositiveInt codL)
    {
        var id = codL.Get + 10000; // Use high ID to ensure it doesn't exist

        var response = _client.GetAsync($"/api/livros/{id}").Result;

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = response.Content.ReadAsStringAsync().Result;
        content.Should().Contain("encontrad");
    }

    /// <summary>
    /// Property: For any request to get a non-existent Autor, the API should return 404 with specific error message.

    /// </summary>
    [Property(MaxTest = 50)]
    public void Autor_NotFound_ShouldReturnSpecificErrorMessage(PositiveInt codAu)
    {
        var id = codAu.Get + 10000; // Use high ID to ensure it doesn't exist

        var response = _client.GetAsync($"/api/autores/{id}").Result;

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = response.Content.ReadAsStringAsync().Result;
        content.Should().Contain("encontrad");
    }

    /// <summary>
    /// Property: For any request to get a non-existent Assunto, the API should return 404 with specific error message.

    /// </summary>
    [Property(MaxTest = 50)]
    public void Assunto_NotFound_ShouldReturnSpecificErrorMessage(PositiveInt codAs)
    {
        var id = codAs.Get + 10000; // Use high ID to ensure it doesn't exist

        var response = _client.GetAsync($"/api/assuntos/{id}").Result;

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = response.Content.ReadAsStringAsync().Result;
        content.Should().Contain("encontrad");
    }
}