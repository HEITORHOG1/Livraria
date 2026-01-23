using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Livraria.Application.Assuntos.Commands.CreateAssunto;
using Livraria.Application.Autores.Commands.CreateAutor;
using Livraria.Application.DTOs;
using Livraria.Application.Livros.Commands.CreateLivro;
using Livraria.Application.Livros.Commands.UpdateLivro;
using Livraria.Domain.Interfaces;
using Livraria.Infrastructure.Data;
using Livraria.Infrastructure.Data.Seed;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace Livraria.Integration.Tests.Properties;

/// <summary>
/// Property-based tests for validation operations.
/// Tests Property 20 from the design document.

/// </summary>
public class ValidationPropertyTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _dbName;

    public ValidationPropertyTests()
    {
        _dbName = $"ValidationTestDb_{Guid.NewGuid()}";
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.UseSetting("RunMigrations", "false");
                builder.UseSetting("SeedData", "false");
                builder.UseSetting("ConnectionStrings:DefaultConnection", "");

                builder.ConfigureServices(services =>
                {
                    // Remove existing DbContext registrations
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    var unitOfWorkDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IUnitOfWork));
                    if (unitOfWorkDescriptor != null)
                        services.Remove(unitOfWorkDescriptor);

                    var dbContextDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(ApplicationDbContext));
                    if (dbContextDescriptor != null)
                        services.Remove(dbContextDescriptor);

                    // Add in-memory database
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(_dbName);
                    });

                    // Re-register IUnitOfWork
                    services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
                });
            });

        _client = _factory.CreateClient();

        // Seed initial data
        SeedTestData();
    }

    private void SeedTestData()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = new Mock<ILogger<DatabaseSeeder>>();
        var seeder = new DatabaseSeeder(context, logger.Object);
        seeder.SeedAsync().GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Helper Methods

    private async Task<AutorDto> CreateAutorAsync(string nome)
    {
        var command = new CreateAutorCommand(nome);
        var response = await _client.PostAsJsonAsync("/api/autores", command);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AutorDto>())!;
    }

    private async Task<AssuntoDto> CreateAssuntoAsync(string descricao)
    {
        var command = new CreateAssuntoCommand(descricao);
        var response = await _client.PostAsJsonAsync("/api/assuntos", command);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AssuntoDto>())!;
    }

    private async Task<LivroDto> CreateLivroAsync(
        string titulo,
        string editora,
        int edicao,
        string anoPublicacao,
        IEnumerable<int> autoresCodAu,
        IEnumerable<int> assuntosCodAs,
        IDictionary<int, decimal> precos)
    {
        var command = new CreateLivroCommand(titulo, editora, edicao, anoPublicacao, autoresCodAu, assuntosCodAs, precos);
        var response = await _client.PostAsJsonAsync("/api/livros", command);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LivroDto>())!;
    }

    private static string GenerateValidTitulo(int seed) =>
        $"Livro Val {seed}"[..Math.Min(40, $"Livro Val {seed}".Length)];

    private static string GenerateValidEditora(int seed) =>
        $"Editora {seed}"[..Math.Min(40, $"Editora {seed}".Length)];

    private static string GenerateValidNome(int seed) =>
        $"Autor Val {seed}"[..Math.Min(40, $"Autor Val {seed}".Length)];

    private static string GenerateValidDescricao(int seed) =>
        $"Assunto {seed}"[..Math.Min(20, $"Assunto {seed}".Length)];

    #endregion Helper Methods



    /// <summary>
    /// Non-Existent Reference Rejection

    ///
    /// For any attempt to associate a non-existent Autor or Assunto with a Livro,
    /// the system should reject the request with a validation error.
    /// </summary>
    [Property(MaxTest = 20)]
    public async Task CreateLivroWithNonExistentAutor_ReturnsValidationError(PositiveInt seed)
    {
        // Arrange - Use high ID that doesn't exist
        var nonExistentAutorId = seed.Get + 100000;

        var command = new CreateLivroCommand(
            GenerateValidTitulo(seed.Get),
            GenerateValidEditora(seed.Get),
            1,
            "2024",
            new[] { nonExistentAutorId },
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        // Act
        var response = await _client.PostAsJsonAsync("/api/livros", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("autor");
    }

    /// <summary>
    /// Creating Livro with non-existent Assunto should return validation error

    /// </summary>
    [Property(MaxTest = 20)]
    public async Task CreateLivroWithNonExistentAssunto_ReturnsValidationError(PositiveInt seed)
    {
        // Arrange - Create a valid autor first
        var autor = await CreateAutorAsync(GenerateValidNome(seed.Get));

        // Use high ID that doesn't exist for assunto
        var nonExistentAssuntoId = seed.Get + 100000;

        var command = new CreateLivroCommand(
            GenerateValidTitulo(seed.Get),
            GenerateValidEditora(seed.Get),
            1,
            "2024",
            new[] { autor.CodAu },
            new[] { nonExistentAssuntoId },
            new Dictionary<int, decimal>());

        // Act
        var response = await _client.PostAsJsonAsync("/api/livros", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("assunto");
    }

    /// <summary>
    /// Updating Livro with non-existent Autor should return validation error

    /// </summary>
    [Property(MaxTest = 20)]
    public async Task UpdateLivroWithNonExistentAutor_ReturnsValidationError(PositiveInt seed)
    {
        // Arrange - Create a valid livro first
        var autor = await CreateAutorAsync(GenerateValidNome(seed.Get + 1000));
        var livro = await CreateLivroAsync(
            GenerateValidTitulo(seed.Get + 1000),
            GenerateValidEditora(seed.Get + 1000),
            1,
            "2024",
            new[] { autor.CodAu },
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        // Use high ID that doesn't exist for autor
        var nonExistentAutorId = seed.Get + 100000;

        var updateCommand = new UpdateLivroCommand(
            livro.CodL,
            livro.Titulo,
            livro.Editora,
            livro.Edicao,
            livro.AnoPublicacao,
            new[] { nonExistentAutorId },
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        // Act
        var response = await _client.PutAsJsonAsync($"/api/livros/{livro.CodL}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("autor");
    }

    /// <summary>
    /// Updating Livro with non-existent Assunto should return validation error

    /// </summary>
    [Property(MaxTest = 20)]
    public async Task UpdateLivroWithNonExistentAssunto_ReturnsValidationError(PositiveInt seed)
    {
        // Arrange - Create a valid livro first
        var autor = await CreateAutorAsync(GenerateValidNome(seed.Get + 2000));
        var livro = await CreateLivroAsync(
            GenerateValidTitulo(seed.Get + 2000),
            GenerateValidEditora(seed.Get + 2000),
            1,
            "2024",
            new[] { autor.CodAu },
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        // Use high ID that doesn't exist for assunto
        var nonExistentAssuntoId = seed.Get + 100000;

        var updateCommand = new UpdateLivroCommand(
            livro.CodL,
            livro.Titulo,
            livro.Editora,
            livro.Edicao,
            livro.AnoPublicacao,
            new[] { autor.CodAu },
            new[] { nonExistentAssuntoId },
            new Dictionary<int, decimal>());

        // Act
        var response = await _client.PutAsJsonAsync($"/api/livros/{livro.CodL}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("assunto");
    }

    /// <summary>
    /// Creating Livro with mix of valid and non-existent Autores should return validation error

    /// </summary>
    [Fact]
    public async Task CreateLivroWithMixedValidAndNonExistentAutores_ReturnsValidationError()
    {
        // Arrange - Create one valid autor
        var validAutor = await CreateAutorAsync("Autor Válido P20");
        var nonExistentAutorId = 999999;

        var command = new CreateLivroCommand(
            "Livro Mix Autores P20",
            "Editora",
            1,
            "2024",
            new[] { validAutor.CodAu, nonExistentAutorId },
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        // Act
        var response = await _client.PostAsJsonAsync("/api/livros", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("autor");
    }

    /// <summary>
    /// Creating Livro with mix of valid and non-existent Assuntos should return validation error

    /// </summary>
    [Fact]
    public async Task CreateLivroWithMixedValidAndNonExistentAssuntos_ReturnsValidationError()
    {
        // Arrange - Create valid autor and assunto
        var autor = await CreateAutorAsync("Autor Para Mix P20");
        var validAssunto = await CreateAssuntoAsync("Assunto Válido P20");
        var nonExistentAssuntoId = 999999;

        var command = new CreateLivroCommand(
            "Livro Mix Assuntos P20",
            "Editora",
            1,
            "2024",
            new[] { autor.CodAu },
            new[] { validAssunto.CodAs, nonExistentAssuntoId },
            new Dictionary<int, decimal>());

        // Act
        var response = await _client.PostAsJsonAsync("/api/livros", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("assunto");
    }

    /// <summary>
    /// Updating Livro with mix of valid and non-existent Autores should return validation error

    /// </summary>
    [Fact]
    public async Task UpdateLivroWithMixedValidAndNonExistentAutores_ReturnsValidationError()
    {
        // Arrange - Create valid livro
        var autor = await CreateAutorAsync("Autor Original P20b");
        var livro = await CreateLivroAsync(
            "Livro Update Mix P20",
            "Editora",
            1,
            "2024",
            new[] { autor.CodAu },
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        var newValidAutor = await CreateAutorAsync("Autor Novo P20b");
        var nonExistentAutorId = 999999;

        var updateCommand = new UpdateLivroCommand(
            livro.CodL,
            livro.Titulo,
            livro.Editora,
            livro.Edicao,
            livro.AnoPublicacao,
            new[] { newValidAutor.CodAu, nonExistentAutorId },
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        // Act
        var response = await _client.PutAsJsonAsync($"/api/livros/{livro.CodL}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("autor");
    }

    /// <summary>
    /// Updating Livro with mix of valid and non-existent Assuntos should return validation error

    /// </summary>
    [Fact]
    public async Task UpdateLivroWithMixedValidAndNonExistentAssuntos_ReturnsValidationError()
    {
        // Arrange - Create valid livro
        var autor = await CreateAutorAsync("Autor Para Update P20c");
        var livro = await CreateLivroAsync(
            "Livro Update Mix Ass P20",
            "Editora",
            1,
            "2024",
            new[] { autor.CodAu },
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        var validAssunto = await CreateAssuntoAsync("Assunto Novo P20c");
        var nonExistentAssuntoId = 999999;

        var updateCommand = new UpdateLivroCommand(
            livro.CodL,
            livro.Titulo,
            livro.Editora,
            livro.Edicao,
            livro.AnoPublicacao,
            new[] { autor.CodAu },
            new[] { validAssunto.CodAs, nonExistentAssuntoId },
            new Dictionary<int, decimal>());

        // Act
        var response = await _client.PutAsJsonAsync($"/api/livros/{livro.CodL}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("assunto");
    }

    /// <summary>
    /// Creating Livro with multiple non-existent Autores should return validation error

    /// </summary>
    [Fact]
    public async Task CreateLivroWithMultipleNonExistentAutores_ReturnsValidationError()
    {
        // Arrange
        var nonExistentAutorIds = new[] { 999997, 999998, 999999 };

        var command = new CreateLivroCommand(
            "Livro Multi NonExist P20",
            "Editora",
            1,
            "2024",
            nonExistentAutorIds,
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        // Act
        var response = await _client.PostAsJsonAsync("/api/livros", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("autor");
    }

    /// <summary>
    /// Creating Livro with multiple non-existent Assuntos should return validation error

    /// </summary>
    [Fact]
    public async Task CreateLivroWithMultipleNonExistentAssuntos_ReturnsValidationError()
    {
        // Arrange
        var autor = await CreateAutorAsync("Autor Multi NonExist P20");
        var nonExistentAssuntoIds = new[] { 999997, 999998, 999999 };

        var command = new CreateLivroCommand(
            "Livro Multi NonExist Ass",
            "Editora",
            1,
            "2024",
            new[] { autor.CodAu },
            nonExistentAssuntoIds,
            new Dictionary<int, decimal>());

        // Act
        var response = await _client.PostAsJsonAsync("/api/livros", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("assunto");
    }

    /// <summary>
    /// Creating Livro with valid references should succeed (positive test)

    /// </summary>
    [Fact]
    public async Task CreateLivroWithValidReferences_Succeeds()
    {
        // Arrange
        var autor = await CreateAutorAsync("Autor Válido Final P20");
        var assunto = await CreateAssuntoAsync("Assunto Válido P20");

        var command = new CreateLivroCommand(
            "Livro Válido P20",
            "Editora",
            1,
            "2024",
            new[] { autor.CodAu },
            new[] { assunto.CodAs },
            new Dictionary<int, decimal>());

        // Act
        var response = await _client.PostAsJsonAsync("/api/livros", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var livro = await response.Content.ReadFromJsonAsync<LivroDto>();
        livro.Should().NotBeNull();
        livro!.Autores.Should().HaveCount(1);
        livro.Assuntos.Should().HaveCount(1);
    }

    /// <summary>
    /// Updating Livro with valid references should succeed (positive test)

    /// </summary>
    [Fact]
    public async Task UpdateLivroWithValidReferences_Succeeds()
    {
        // Arrange
        var autor1 = await CreateAutorAsync("Autor Original Final P20");
        var livro = await CreateLivroAsync(
            "Livro Para Update Final",
            "Editora",
            1,
            "2024",
            new[] { autor1.CodAu },
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        var autor2 = await CreateAutorAsync("Autor Novo Final P20");
        var assunto = await CreateAssuntoAsync("Assunto Final P20");

        var updateCommand = new UpdateLivroCommand(
            livro.CodL,
            livro.Titulo,
            livro.Editora,
            livro.Edicao,
            livro.AnoPublicacao,
            new[] { autor2.CodAu },
            new[] { assunto.CodAs },
            new Dictionary<int, decimal>());

        // Act
        var response = await _client.PutAsJsonAsync($"/api/livros/{livro.CodL}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedLivro = await response.Content.ReadFromJsonAsync<LivroDto>();
        updatedLivro.Should().NotBeNull();
        updatedLivro!.Autores.Should().HaveCount(1);
        updatedLivro.Autores.First().CodAu.Should().Be(autor2.CodAu);
        updatedLivro.Assuntos.Should().HaveCount(1);
        updatedLivro.Assuntos.First().CodAs.Should().Be(assunto.CodAs);
    }


}
