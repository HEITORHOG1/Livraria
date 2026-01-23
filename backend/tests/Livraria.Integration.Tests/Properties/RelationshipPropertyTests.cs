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
/// Property-based tests for relationship operations.
/// Tests Properties 9-13 from the design document.

/// </summary>
public class RelationshipPropertyTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _dbName;

    public RelationshipPropertyTests()
    {
        _dbName = $"RelationshipTestDb_{Guid.NewGuid()}";
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

    private async Task<LivroDto?> GetLivroByIdAsync(int codL)
    {
        var response = await _client.GetAsync($"/api/livros/{codL}");
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LivroDto>();
    }

    private async Task<IEnumerable<FormaCompraDto>> GetFormasCompraAsync()
    {
        var response = await _client.GetAsync("/api/formas-compra");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IEnumerable<FormaCompraDto>>())!;
    }

    private static string GenerateValidTitulo(int seed) =>
        $"Livro Rel {seed}".Substring(0, Math.Min(40, $"Livro Rel {seed}".Length));

    private static string GenerateValidEditora(int seed) =>
        $"Editora {seed}".Substring(0, Math.Min(40, $"Editora {seed}".Length));

    private static string GenerateValidNome(int seed) =>
        $"Autor Rel {seed}".Substring(0, Math.Min(40, $"Autor Rel {seed}".Length));

    private static string GenerateValidDescricao(int seed) =>
        $"Assunto {seed}".Substring(0, Math.Min(20, $"Assunto {seed}".Length));

    #endregion Helper Methods

    #region Livro-Autor Association Creation

    /// <summary>
    /// Livro-Autor Association Creation

    ///
    /// For any Livro created with a list of valid Autor codes, the resulting Livro
    /// should have exactly those Autores associated.
    /// </summary>
    [Property(MaxTest = 20)]
    public async Task LivroAutorAssociationCreation_HasExactAutores(PositiveInt seed)
    {
        // Arrange - Create multiple autores
        var autor1 = await CreateAutorAsync(GenerateValidNome(seed.Get));
        var autor2 = await CreateAutorAsync(GenerateValidNome(seed.Get + 1));
        var autor3 = await CreateAutorAsync(GenerateValidNome(seed.Get + 2));
        var autoresCodAu = new[] { autor1.CodAu, autor2.CodAu, autor3.CodAu };

        // Act - Create livro with all autores
        var livro = await CreateLivroAsync(
            GenerateValidTitulo(seed.Get),
            GenerateValidEditora(seed.Get),
            1,
            "2024",
            autoresCodAu,
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        // Assert - Retrieve and verify associations
        var retrievedLivro = await GetLivroByIdAsync(livro.CodL);

        retrievedLivro.Should().NotBeNull();
        retrievedLivro!.Autores.Should().HaveCount(3);
        retrievedLivro.Autores.Select(a => a.CodAu).Should().BeEquivalentTo(autoresCodAu);
    }

    /// <summary>
    /// Livro with single autor should have exactly one autor

    /// </summary>
    [Fact]
    public async Task LivroWithSingleAutor_HasExactlyOneAutor()
    {
        // Arrange
        var autor = await CreateAutorAsync("Autor Único P9");

        // Act
        var livro = await CreateLivroAsync(
            "Livro Um Autor P9",
            "Editora",
            1,
            "2024",
            new[] { autor.CodAu },
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        var retrievedLivro = await GetLivroByIdAsync(livro.CodL);

        // Assert
        retrievedLivro.Should().NotBeNull();
        retrievedLivro!.Autores.Should().HaveCount(1);
        retrievedLivro.Autores.First().CodAu.Should().Be(autor.CodAu);
        retrievedLivro.Autores.First().Nome.Should().Be("Autor Único P9");
    }

    /// <summary>
    /// Livro with multiple autores preserves all autor data

    /// </summary>
    [Fact]
    public async Task LivroWithMultipleAutores_PreservesAllAutorData()
    {
        // Arrange
        var autor1 = await CreateAutorAsync("Primeiro Autor P9");
        var autor2 = await CreateAutorAsync("Segundo Autor P9");
        var autoresCodAu = new[] { autor1.CodAu, autor2.CodAu };

        // Act
        var livro = await CreateLivroAsync(
            "Livro Multi Autor P9",
            "Editora",
            1,
            "2024",
            autoresCodAu,
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        var retrievedLivro = await GetLivroByIdAsync(livro.CodL);

        // Assert
        retrievedLivro.Should().NotBeNull();
        retrievedLivro!.Autores.Should().HaveCount(2);

        var autorNomes = retrievedLivro.Autores.Select(a => a.Nome).ToList();
        autorNomes.Should().Contain("Primeiro Autor P9");
        autorNomes.Should().Contain("Segundo Autor P9");
    }

    #endregion Livro-Autor Association Creation

    #region Livro-Autor Association Update Replaces All

    /// <summary>
    /// Livro-Autor Association Update Replaces All

    ///
    /// For any existing Livro with Autores, updating with a new list of Autor codes
    /// should result in exactly the new Autores being associated (old associations removed).
    /// </summary>
    [Property(MaxTest = 20)]
    public async Task LivroAutorUpdateReplacesAll(PositiveInt seed)
    {
        // Arrange - Create initial autores
        var autorOriginal1 = await CreateAutorAsync(GenerateValidNome(seed.Get + 100));
        var autorOriginal2 = await CreateAutorAsync(GenerateValidNome(seed.Get + 101));
        var originalAutores = new[] { autorOriginal1.CodAu, autorOriginal2.CodAu };

        // Create livro with original autores
        var livro = await CreateLivroAsync(
            GenerateValidTitulo(seed.Get + 100),
            GenerateValidEditora(seed.Get + 100),
            1,
            "2024",
            originalAutores,
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        // Create new autores for update
        var autorNovo1 = await CreateAutorAsync(GenerateValidNome(seed.Get + 200));
        var autorNovo2 = await CreateAutorAsync(GenerateValidNome(seed.Get + 201));
        var autorNovo3 = await CreateAutorAsync(GenerateValidNome(seed.Get + 202));
        var newAutores = new[] { autorNovo1.CodAu, autorNovo2.CodAu, autorNovo3.CodAu };

        // Act - Update livro with new autores
        var updateCommand = new UpdateLivroCommand(
            livro.CodL,
            livro.Titulo,
            livro.Editora,
            livro.Edicao,
            livro.AnoPublicacao,
            newAutores,
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        var updateResponse = await _client.PutAsJsonAsync($"/api/livros/{livro.CodL}", updateCommand);
        updateResponse.EnsureSuccessStatusCode();

        // Assert - Retrieve and verify only new autores exist
        var retrievedLivro = await GetLivroByIdAsync(livro.CodL);

        retrievedLivro.Should().NotBeNull();
        retrievedLivro!.Autores.Should().HaveCount(3);
        retrievedLivro.Autores.Select(a => a.CodAu).Should().BeEquivalentTo(newAutores);
        retrievedLivro.Autores.Select(a => a.CodAu).Should().NotContain(autorOriginal1.CodAu);
        retrievedLivro.Autores.Select(a => a.CodAu).Should().NotContain(autorOriginal2.CodAu);
    }

    /// <summary>
    /// Update from multiple autores to single autor

    /// </summary>
    [Fact]
    public async Task UpdateFromMultipleToSingleAutor_ReplacesAll()
    {
        // Arrange
        var autor1 = await CreateAutorAsync("Autor Original 1 P10");
        var autor2 = await CreateAutorAsync("Autor Original 2 P10");
        var autorNovo = await CreateAutorAsync("Autor Novo P10");

        var livro = await CreateLivroAsync(
            "Livro Update Autor P10",
            "Editora",
            1,
            "2024",
            new[] { autor1.CodAu, autor2.CodAu },
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        // Act - Update to single autor
        var updateCommand = new UpdateLivroCommand(
            livro.CodL,
            livro.Titulo,
            livro.Editora,
            livro.Edicao,
            livro.AnoPublicacao,
            new[] { autorNovo.CodAu },
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        var updateResponse = await _client.PutAsJsonAsync($"/api/livros/{livro.CodL}", updateCommand);
        updateResponse.EnsureSuccessStatusCode();

        var retrievedLivro = await GetLivroByIdAsync(livro.CodL);

        // Assert
        retrievedLivro.Should().NotBeNull();
        retrievedLivro!.Autores.Should().HaveCount(1);
        retrievedLivro.Autores.First().CodAu.Should().Be(autorNovo.CodAu);
        retrievedLivro.Autores.First().Nome.Should().Be("Autor Novo P10");
    }

    /// <summary>
    /// Update from single autor to multiple autores

    /// </summary>
    [Fact]
    public async Task UpdateFromSingleToMultipleAutores_ReplacesAll()
    {
        // Arrange
        var autorOriginal = await CreateAutorAsync("Autor Original P10b");
        var autorNovo1 = await CreateAutorAsync("Autor Novo 1 P10b");
        var autorNovo2 = await CreateAutorAsync("Autor Novo 2 P10b");

        var livro = await CreateLivroAsync(
            "Livro Single to Multi P10",
            "Editora",
            1,
            "2024",
            new[] { autorOriginal.CodAu },
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        // Act - Update to multiple autores
        var updateCommand = new UpdateLivroCommand(
            livro.CodL,
            livro.Titulo,
            livro.Editora,
            livro.Edicao,
            livro.AnoPublicacao,
            new[] { autorNovo1.CodAu, autorNovo2.CodAu },
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        var updateResponse = await _client.PutAsJsonAsync($"/api/livros/{livro.CodL}", updateCommand);
        updateResponse.EnsureSuccessStatusCode();

        var retrievedLivro = await GetLivroByIdAsync(livro.CodL);

        // Assert
        retrievedLivro.Should().NotBeNull();
        retrievedLivro!.Autores.Should().HaveCount(2);
        retrievedLivro.Autores.Select(a => a.CodAu).Should().BeEquivalentTo(new[] { autorNovo1.CodAu, autorNovo2.CodAu });
        retrievedLivro.Autores.Select(a => a.CodAu).Should().NotContain(autorOriginal.CodAu);
    }

    #endregion Livro-Autor Association Update Replaces All

    #region Livro-Assunto Association Creation

    /// <summary>
    /// Livro-Assunto Association Creation

    ///
    /// For any Livro created with a list of valid Assunto codes, the resulting Livro
    /// should have exactly those Assuntos associated.
    /// </summary>
    [Property(MaxTest = 20)]
    public async Task LivroAssuntoAssociationCreation_HasExactAssuntos(PositiveInt seed)
    {
        // Arrange - Create autor (required) and multiple assuntos
        var autor = await CreateAutorAsync(GenerateValidNome(seed.Get + 300));
        var assunto1 = await CreateAssuntoAsync(GenerateValidDescricao(seed.Get));
        var assunto2 = await CreateAssuntoAsync(GenerateValidDescricao(seed.Get + 1));
        var assunto3 = await CreateAssuntoAsync(GenerateValidDescricao(seed.Get + 2));
        var assuntosCodAs = new[] { assunto1.CodAs, assunto2.CodAs, assunto3.CodAs };

        // Act - Create livro with all assuntos
        var livro = await CreateLivroAsync(
            GenerateValidTitulo(seed.Get + 300),
            GenerateValidEditora(seed.Get + 300),
            1,
            "2024",
            new[] { autor.CodAu },
            assuntosCodAs,
            new Dictionary<int, decimal>());

        // Assert - Retrieve and verify associations
        var retrievedLivro = await GetLivroByIdAsync(livro.CodL);

        retrievedLivro.Should().NotBeNull();
        retrievedLivro!.Assuntos.Should().HaveCount(3);
        retrievedLivro.Assuntos.Select(a => a.CodAs).Should().BeEquivalentTo(assuntosCodAs);
    }

    /// <summary>
    /// Livro with single assunto should have exactly one assunto

    /// </summary>
    [Fact]
    public async Task LivroWithSingleAssunto_HasExactlyOneAssunto()
    {
        // Arrange
        var autor = await CreateAutorAsync("Autor P11");
        var assunto = await CreateAssuntoAsync("Assunto Único P11");

        // Act
        var livro = await CreateLivroAsync(
            "Livro Um Assunto P11",
            "Editora",
            1,
            "2024",
            new[] { autor.CodAu },
            new[] { assunto.CodAs },
            new Dictionary<int, decimal>());

        var retrievedLivro = await GetLivroByIdAsync(livro.CodL);

        // Assert
        retrievedLivro.Should().NotBeNull();
        retrievedLivro!.Assuntos.Should().HaveCount(1);
        retrievedLivro.Assuntos.First().CodAs.Should().Be(assunto.CodAs);
        retrievedLivro.Assuntos.First().Descricao.Should().Be("Assunto Único P11");
    }

    /// <summary>
    /// Livro with no assuntos should have empty assuntos list

    /// </summary>
    [Fact]
    public async Task LivroWithNoAssuntos_HasEmptyAssuntosList()
    {
        // Arrange
        var autor = await CreateAutorAsync("Autor Sem Assunto P11");

        // Act
        var livro = await CreateLivroAsync(
            "Livro Sem Assuntos P11",
            "Editora",
            1,
            "2024",
            new[] { autor.CodAu },
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        var retrievedLivro = await GetLivroByIdAsync(livro.CodL);

        // Assert
        retrievedLivro.Should().NotBeNull();
        retrievedLivro!.Assuntos.Should().BeEmpty();
    }

    /// <summary>
    /// Livro with multiple assuntos preserves all assunto data

    /// </summary>
    [Fact]
    public async Task LivroWithMultipleAssuntos_PreservesAllAssuntoData()
    {
        // Arrange
        var autor = await CreateAutorAsync("Autor Multi Ass P11");
        var assunto1 = await CreateAssuntoAsync("Primeiro P11");
        var assunto2 = await CreateAssuntoAsync("Segundo P11");
        var assuntosCodAs = new[] { assunto1.CodAs, assunto2.CodAs };

        // Act
        var livro = await CreateLivroAsync(
            "Livro Multi Assunto P11",
            "Editora",
            1,
            "2024",
            new[] { autor.CodAu },
            assuntosCodAs,
            new Dictionary<int, decimal>());

        var retrievedLivro = await GetLivroByIdAsync(livro.CodL);

        // Assert
        retrievedLivro.Should().NotBeNull();
        retrievedLivro!.Assuntos.Should().HaveCount(2);

        var assuntoDescricoes = retrievedLivro.Assuntos.Select(a => a.Descricao).ToList();
        assuntoDescricoes.Should().Contain("Primeiro P11");
        assuntoDescricoes.Should().Contain("Segundo P11");
    }

    #endregion Livro-Assunto Association Creation

    #region Livro-Assunto Association Update Replaces All

    /// <summary>
    /// Livro-Assunto Association Update Replaces All

    ///
    /// For any existing Livro with Assuntos, updating with a new list of Assunto codes
    /// should result in exactly the new Assuntos being associated (old associations removed).
    /// </summary>
    [Property(MaxTest = 20)]
    public async Task LivroAssuntoUpdateReplacesAll(PositiveInt seed)
    {
        // Arrange - Create autor and initial assuntos
        var autor = await CreateAutorAsync(GenerateValidNome(seed.Get + 400));
        var assuntoOriginal1 = await CreateAssuntoAsync(GenerateValidDescricao(seed.Get + 100));
        var assuntoOriginal2 = await CreateAssuntoAsync(GenerateValidDescricao(seed.Get + 101));
        var originalAssuntos = new[] { assuntoOriginal1.CodAs, assuntoOriginal2.CodAs };

        // Create livro with original assuntos
        var livro = await CreateLivroAsync(
            GenerateValidTitulo(seed.Get + 400),
            GenerateValidEditora(seed.Get + 400),
            1,
            "2024",
            new[] { autor.CodAu },
            originalAssuntos,
            new Dictionary<int, decimal>());

        // Create new assuntos for update
        var assuntoNovo1 = await CreateAssuntoAsync(GenerateValidDescricao(seed.Get + 200));
        var assuntoNovo2 = await CreateAssuntoAsync(GenerateValidDescricao(seed.Get + 201));
        var assuntoNovo3 = await CreateAssuntoAsync(GenerateValidDescricao(seed.Get + 202));
        var newAssuntos = new[] { assuntoNovo1.CodAs, assuntoNovo2.CodAs, assuntoNovo3.CodAs };

        // Act - Update livro with new assuntos
        var updateCommand = new UpdateLivroCommand(
            livro.CodL,
            livro.Titulo,
            livro.Editora,
            livro.Edicao,
            livro.AnoPublicacao,
            new[] { autor.CodAu },
            newAssuntos,
            new Dictionary<int, decimal>());

        var updateResponse = await _client.PutAsJsonAsync($"/api/livros/{livro.CodL}", updateCommand);
        updateResponse.EnsureSuccessStatusCode();

        // Assert - Retrieve and verify only new assuntos exist
        var retrievedLivro = await GetLivroByIdAsync(livro.CodL);

        retrievedLivro.Should().NotBeNull();
        retrievedLivro!.Assuntos.Should().HaveCount(3);
        retrievedLivro.Assuntos.Select(a => a.CodAs).Should().BeEquivalentTo(newAssuntos);
        retrievedLivro.Assuntos.Select(a => a.CodAs).Should().NotContain(assuntoOriginal1.CodAs);
        retrievedLivro.Assuntos.Select(a => a.CodAs).Should().NotContain(assuntoOriginal2.CodAs);
    }

    /// <summary>
    /// Update from multiple assuntos to single assunto

    /// </summary>
    [Fact]
    public async Task UpdateFromMultipleToSingleAssunto_ReplacesAll()
    {
        // Arrange
        var autor = await CreateAutorAsync("Autor P12");
        var assunto1 = await CreateAssuntoAsync("Assunto Orig 1 P12");
        var assunto2 = await CreateAssuntoAsync("Assunto Orig 2 P12");
        var assuntoNovo = await CreateAssuntoAsync("Assunto Novo P12");

        var livro = await CreateLivroAsync(
            "Livro Update Assunto P12",
            "Editora",
            1,
            "2024",
            new[] { autor.CodAu },
            new[] { assunto1.CodAs, assunto2.CodAs },
            new Dictionary<int, decimal>());

        // Act - Update to single assunto
        var updateCommand = new UpdateLivroCommand(
            livro.CodL,
            livro.Titulo,
            livro.Editora,
            livro.Edicao,
            livro.AnoPublicacao,
            new[] { autor.CodAu },
            new[] { assuntoNovo.CodAs },
            new Dictionary<int, decimal>());

        var updateResponse = await _client.PutAsJsonAsync($"/api/livros/{livro.CodL}", updateCommand);
        updateResponse.EnsureSuccessStatusCode();

        var retrievedLivro = await GetLivroByIdAsync(livro.CodL);

        // Assert
        retrievedLivro.Should().NotBeNull();
        retrievedLivro!.Assuntos.Should().HaveCount(1);
        retrievedLivro.Assuntos.First().CodAs.Should().Be(assuntoNovo.CodAs);
        retrievedLivro.Assuntos.First().Descricao.Should().Be("Assunto Novo P12");
    }

    /// <summary>
    /// Update from assuntos to no assuntos

    /// </summary>
    [Fact]
    public async Task UpdateFromAssuntosToNoAssuntos_RemovesAll()
    {
        // Arrange
        var autor = await CreateAutorAsync("Autor Remove Ass P12");
        var assunto1 = await CreateAssuntoAsync("Assunto Remove P12");
        var assunto2 = await CreateAssuntoAsync("Assunto Remove2 P12");

        var livro = await CreateLivroAsync(
            "Livro Remove Assuntos P12",
            "Editora",
            1,
            "2024",
            new[] { autor.CodAu },
            new[] { assunto1.CodAs, assunto2.CodAs },
            new Dictionary<int, decimal>());

        // Act - Update to no assuntos
        var updateCommand = new UpdateLivroCommand(
            livro.CodL,
            livro.Titulo,
            livro.Editora,
            livro.Edicao,
            livro.AnoPublicacao,
            new[] { autor.CodAu },
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        var updateResponse = await _client.PutAsJsonAsync($"/api/livros/{livro.CodL}", updateCommand);
        updateResponse.EnsureSuccessStatusCode();

        var retrievedLivro = await GetLivroByIdAsync(livro.CodL);

        // Assert
        retrievedLivro.Should().NotBeNull();
        retrievedLivro!.Assuntos.Should().BeEmpty();
    }

    #endregion Livro-Assunto Association Update Replaces All

    #region Livro-Preco Association Management

    /// <summary>
    /// Livro-Preco Association Management

    ///
    /// For any Livro created or updated with prices for FormaCompra options, the resulting
    /// Livro should have exactly those prices associated with correct values.
    /// </summary>
    [Property(MaxTest = 20)]
    public async Task LivroPrecoAssociationCreation_HasExactPrecos(PositiveInt seed)
    {
        // Arrange
        var autor = await CreateAutorAsync(GenerateValidNome(seed.Get + 500));
        var formasCompra = (await GetFormasCompraAsync()).ToList();

        // Create precos for all formas de compra
        var precos = new Dictionary<int, decimal>();
        decimal baseValue = 10.00m + seed.Get;
        foreach (var forma in formasCompra)
        {
            precos[forma.CodFc] = baseValue;
            baseValue += 5.00m;
        }

        // Act - Create livro with precos
        var livro = await CreateLivroAsync(
            GenerateValidTitulo(seed.Get + 500),
            GenerateValidEditora(seed.Get + 500),
            1,
            "2024",
            new[] { autor.CodAu },
            Array.Empty<int>(),
            precos);

        // Assert - Retrieve and verify precos
        var retrievedLivro = await GetLivroByIdAsync(livro.CodL);

        retrievedLivro.Should().NotBeNull();
        retrievedLivro!.Precos.Should().HaveCount(precos.Count);

        foreach (var preco in retrievedLivro.Precos)
        {
            precos.Should().ContainKey(preco.FormaCompra_CodFc);
            preco.Valor.Should().Be(precos[preco.FormaCompra_CodFc]);
        }
    }

    /// <summary>
    /// Livro with single preco should have exactly one preco

    /// </summary>
    [Fact]
    public async Task LivroWithSinglePreco_HasExactlyOnePreco()
    {
        // Arrange
        var autor = await CreateAutorAsync("Autor Preco P13");
        var formasCompra = (await GetFormasCompraAsync()).ToList();
        var forma = formasCompra.First();
        var precos = new Dictionary<int, decimal> { { forma.CodFc, 29.90m } };

        // Act
        var livro = await CreateLivroAsync(
            "Livro Um Preco P13",
            "Editora",
            1,
            "2024",
            new[] { autor.CodAu },
            Array.Empty<int>(),
            precos);

        var retrievedLivro = await GetLivroByIdAsync(livro.CodL);

        // Assert
        retrievedLivro.Should().NotBeNull();
        retrievedLivro!.Precos.Should().HaveCount(1);
        retrievedLivro.Precos.First().FormaCompra_CodFc.Should().Be(forma.CodFc);
        retrievedLivro.Precos.First().Valor.Should().Be(29.90m);
    }

    /// <summary>
    /// Livro with no precos should have empty precos list

    /// </summary>
    [Fact]
    public async Task LivroWithNoPrecos_HasEmptyPrecosList()
    {
        // Arrange
        var autor = await CreateAutorAsync("Autor Sem Preco P13");

        // Act
        var livro = await CreateLivroAsync(
            "Livro Sem Precos P13",
            "Editora",
            1,
            "2024",
            new[] { autor.CodAu },
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        var retrievedLivro = await GetLivroByIdAsync(livro.CodL);

        // Assert
        retrievedLivro.Should().NotBeNull();
        retrievedLivro!.Precos.Should().BeEmpty();
    }

    /// <summary>
    /// Update precos replaces all existing precos

    /// </summary>
    [Fact]
    public async Task UpdatePrecosReplacesAll()
    {
        // Arrange
        var autor = await CreateAutorAsync("Autor Update Preco P13");
        var formasCompra = (await GetFormasCompraAsync()).ToList();
        var forma1 = formasCompra[0];
        var forma2 = formasCompra[1];

        var initialPrecos = new Dictionary<int, decimal> { { forma1.CodFc, 10.00m } };
        var livro = await CreateLivroAsync(
            "Livro Update Preco P13",
            "Editora",
            1,
            "2024",
            new[] { autor.CodAu },
            Array.Empty<int>(),
            initialPrecos);

        // Act - Update with new precos
        var newPrecos = new Dictionary<int, decimal> { { forma2.CodFc, 25.50m } };
        var updateCommand = new UpdateLivroCommand(
            livro.CodL,
            livro.Titulo,
            livro.Editora,
            livro.Edicao,
            livro.AnoPublicacao,
            new[] { autor.CodAu },
            Array.Empty<int>(),
            newPrecos);

        var updateResponse = await _client.PutAsJsonAsync($"/api/livros/{livro.CodL}", updateCommand);
        updateResponse.EnsureSuccessStatusCode();

        var retrievedLivro = await GetLivroByIdAsync(livro.CodL);

        // Assert
        retrievedLivro.Should().NotBeNull();
        retrievedLivro!.Precos.Should().HaveCount(1);
        retrievedLivro.Precos.First().FormaCompra_CodFc.Should().Be(forma2.CodFc);
        retrievedLivro.Precos.First().Valor.Should().Be(25.50m);
    }

    /// <summary>
    /// Update precos with multiple formas de compra

    /// </summary>
    [Fact]
    public async Task UpdatePrecosWithMultipleFormas()
    {
        // Arrange
        var autor = await CreateAutorAsync("Autor Multi Preco P13");
        var formasCompra = (await GetFormasCompraAsync()).ToList();
        var forma1 = formasCompra[0];
        var forma2 = formasCompra[1];
        var forma3 = formasCompra.Count > 2 ? formasCompra[2] : formasCompra[0];

        var initialPrecos = new Dictionary<int, decimal> { { forma1.CodFc, 10.00m } };
        var livro = await CreateLivroAsync(
            "Livro Multi Preco P13",
            "Editora",
            1,
            "2024",
            new[] { autor.CodAu },
            Array.Empty<int>(),
            initialPrecos);

        // Act - Update with multiple precos
        var newPrecos = new Dictionary<int, decimal>
        {
            { forma1.CodFc, 15.00m },
            { forma2.CodFc, 20.00m }
        };

        if (forma3.CodFc != forma1.CodFc)
        {
            newPrecos[forma3.CodFc] = 25.00m;
        }

        var updateCommand = new UpdateLivroCommand(
            livro.CodL,
            livro.Titulo,
            livro.Editora,
            livro.Edicao,
            livro.AnoPublicacao,
            new[] { autor.CodAu },
            Array.Empty<int>(),
            newPrecos);

        var updateResponse = await _client.PutAsJsonAsync($"/api/livros/{livro.CodL}", updateCommand);
        updateResponse.EnsureSuccessStatusCode();

        var retrievedLivro = await GetLivroByIdAsync(livro.CodL);

        // Assert
        retrievedLivro.Should().NotBeNull();
        retrievedLivro!.Precos.Should().HaveCount(newPrecos.Count);

        foreach (var preco in retrievedLivro.Precos)
        {
            newPrecos.Should().ContainKey(preco.FormaCompra_CodFc);
            preco.Valor.Should().Be(newPrecos[preco.FormaCompra_CodFc]);
        }
    }

    /// <summary>
    /// Preco values are stored with correct decimal precision

    /// </summary>
    [Fact]
    public async Task PrecoValuesStoredWithCorrectPrecision()
    {
        // Arrange
        var autor = await CreateAutorAsync("Autor Precision P13");
        var formasCompra = (await GetFormasCompraAsync()).ToList();
        var forma = formasCompra.First();
        var precos = new Dictionary<int, decimal> { { forma.CodFc, 123.45m } };

        // Act
        var livro = await CreateLivroAsync(
            "Livro Precision P13",
            "Editora",
            1,
            "2024",
            new[] { autor.CodAu },
            Array.Empty<int>(),
            precos);

        var retrievedLivro = await GetLivroByIdAsync(livro.CodL);

        // Assert
        retrievedLivro.Should().NotBeNull();
        retrievedLivro!.Precos.Should().HaveCount(1);
        retrievedLivro.Precos.First().Valor.Should().Be(123.45m);
    }

    /// <summary>
    /// Update removes precos when empty dictionary provided

    /// </summary>
    [Fact]
    public async Task UpdateWithEmptyPrecos_RemovesAllPrecos()
    {
        // Arrange
        var autor = await CreateAutorAsync("Autor Remove Preco P13");
        var formasCompra = (await GetFormasCompraAsync()).ToList();
        var forma1 = formasCompra[0];
        var forma2 = formasCompra[1];

        var initialPrecos = new Dictionary<int, decimal>
        {
            { forma1.CodFc, 10.00m },
            { forma2.CodFc, 20.00m }
        };

        var livro = await CreateLivroAsync(
            "Livro Remove Precos P13",
            "Editora",
            1,
            "2024",
            new[] { autor.CodAu },
            Array.Empty<int>(),
            initialPrecos);

        // Act - Update with empty precos
        var updateCommand = new UpdateLivroCommand(
            livro.CodL,
            livro.Titulo,
            livro.Editora,
            livro.Edicao,
            livro.AnoPublicacao,
            new[] { autor.CodAu },
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        var updateResponse = await _client.PutAsJsonAsync($"/api/livros/{livro.CodL}", updateCommand);
        updateResponse.EnsureSuccessStatusCode();

        var retrievedLivro = await GetLivroByIdAsync(livro.CodL);

        // Assert
        retrievedLivro.Should().NotBeNull();
        retrievedLivro!.Precos.Should().BeEmpty();
    }

    #endregion Livro-Preco Association Management
}
