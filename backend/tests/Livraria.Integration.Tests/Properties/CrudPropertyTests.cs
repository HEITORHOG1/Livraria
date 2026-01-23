using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Livraria.Application.Assuntos.Commands.CreateAssunto;
using Livraria.Application.Assuntos.Commands.UpdateAssunto;
using Livraria.Application.Autores.Commands.CreateAutor;
using Livraria.Application.Autores.Commands.UpdateAutor;
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
/// Property-based tests for CRUD operations.
/// Tests Properties 4-8 from the design document.

/// </summary>
public class CrudPropertyTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _dbName;

    public CrudPropertyTests()
    {
        _dbName = $"CrudTestDb_{Guid.NewGuid()}";
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

    private async Task<AutorDto?> GetAutorByIdAsync(int codAu)
    {
        var response = await _client.GetAsync($"/api/autores/{codAu}");
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AutorDto>();
    }

    private async Task<AssuntoDto?> GetAssuntoByIdAsync(int codAs)
    {
        var response = await _client.GetAsync($"/api/assuntos/{codAs}");
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AssuntoDto>();
    }

    private async Task<IEnumerable<FormaCompraDto>> GetFormasCompraAsync()
    {
        var response = await _client.GetAsync("/api/formas-compra");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IEnumerable<FormaCompraDto>>())!;
    }

    private static string GenerateValidTitulo(int seed) =>
        $"Livro Test {seed}".Substring(0, Math.Min(40, $"Livro Test {seed}".Length));

    private static string GenerateValidEditora(int seed) =>
        $"Editora {seed}".Substring(0, Math.Min(40, $"Editora {seed}".Length));

    private static string GenerateValidNome(int seed) =>
        $"Autor Test {seed}".Substring(0, Math.Min(40, $"Autor Test {seed}".Length));

    private static string GenerateValidDescricao(int seed) =>
        $"Assunto {seed}".Substring(0, Math.Min(20, $"Assunto {seed}".Length));

    #endregion Helper Methods

    #region Livro Retrieval Includes All Relationships

    /// <summary>
    /// Livro Retrieval Includes All Relationships

    ///
    /// For any Livro with associated Autores, Assuntos, and Precos, retrieving the Livro
    /// should include all associated entities in the response.
    /// </summary>
    [Property(MaxTest = 20)]
    public async Task LivroRetrievalIncludesAllRelationships(PositiveInt seed)
    {
        // Arrange - Create autores
        var autor1 = await CreateAutorAsync(GenerateValidNome(seed.Get));
        var autor2 = await CreateAutorAsync(GenerateValidNome(seed.Get + 1));
        var autoresCodAu = new[] { autor1.CodAu, autor2.CodAu };

        // Create assuntos
        var assunto1 = await CreateAssuntoAsync(GenerateValidDescricao(seed.Get));
        var assunto2 = await CreateAssuntoAsync(GenerateValidDescricao(seed.Get + 1));
        var assuntosCodAs = new[] { assunto1.CodAs, assunto2.CodAs };

        // Get formas de compra and create precos
        var formasCompra = (await GetFormasCompraAsync()).ToList();
        var precos = new Dictionary<int, decimal>();
        foreach (var forma in formasCompra.Take(2))
        {
            precos[forma.CodFc] = 10.00m + seed.Get;
        }

        // Create livro with all relationships
        var livro = await CreateLivroAsync(
            GenerateValidTitulo(seed.Get),
            GenerateValidEditora(seed.Get),
            1,
            "2024",
            autoresCodAu,
            assuntosCodAs,
            precos);

        // Act - Retrieve livro
        var retrievedLivro = await GetLivroByIdAsync(livro.CodL);

        // Assert
        retrievedLivro.Should().NotBeNull();
        retrievedLivro!.Autores.Should().HaveCount(2);
        retrievedLivro.Autores.Select(a => a.CodAu).Should().BeEquivalentTo(autoresCodAu);
        retrievedLivro.Assuntos.Should().HaveCount(2);
        retrievedLivro.Assuntos.Select(a => a.CodAs).Should().BeEquivalentTo(assuntosCodAs);
        retrievedLivro.Precos.Should().HaveCount(precos.Count);
        foreach (var preco in retrievedLivro.Precos)
        {
            precos.Should().ContainKey(preco.FormaCompra_CodFc);
            preco.Valor.Should().Be(precos[preco.FormaCompra_CodFc]);
        }
    }

    /// <summary>
    /// Livro with single autor should include that autor

    /// </summary>
    [Fact]
    public async Task LivroWithSingleAutor_ShouldIncludeAutor()
    {
        // Arrange
        var autor = await CreateAutorAsync("Autor Único");
        var livro = await CreateLivroAsync(
            "Livro com Um Autor",
            "Editora",
            1,
            "2024",
            new[] { autor.CodAu },
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        // Act
        var retrievedLivro = await GetLivroByIdAsync(livro.CodL);

        // Assert
        retrievedLivro.Should().NotBeNull();
        retrievedLivro!.Autores.Should().HaveCount(1);
        retrievedLivro.Autores.First().CodAu.Should().Be(autor.CodAu);
        retrievedLivro.Autores.First().Nome.Should().Be("Autor Único");
    }

    /// <summary>
    /// Livro with no assuntos should return empty assuntos list

    /// </summary>
    [Fact]
    public async Task LivroWithNoAssuntos_ShouldReturnEmptyAssuntosList()
    {
        // Arrange
        var autor = await CreateAutorAsync("Autor Sem Assunto");
        var livro = await CreateLivroAsync(
            "Livro Sem Assuntos",
            "Editora",
            1,
            "2024",
            new[] { autor.CodAu },
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        // Act
        var retrievedLivro = await GetLivroByIdAsync(livro.CodL);

        // Assert
        retrievedLivro.Should().NotBeNull();
        retrievedLivro!.Assuntos.Should().BeEmpty();
    }

    #endregion Livro Retrieval Includes All Relationships

    #region Livro Update Persists Changes

    /// <summary>
    /// Livro Update Persists Changes

    ///
    /// For any existing Livro and valid update data, updating the Livro and then
    /// retrieving it should return the updated data.
    /// </summary>
    [Property(MaxTest = 20)]
    public async Task LivroUpdatePersistsChanges(PositiveInt seed)
    {
        // Arrange - Create initial livro
        var autor = await CreateAutorAsync(GenerateValidNome(seed.Get));
        var livro = await CreateLivroAsync(
            GenerateValidTitulo(seed.Get),
            GenerateValidEditora(seed.Get),
            1,
            "2020",
            new[] { autor.CodAu },
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        // Create new data for update
        var newTitulo = GenerateValidTitulo(seed.Get + 100);
        var newEditora = GenerateValidEditora(seed.Get + 100);
        var newEdicao = 2;
        var newAno = "2024";

        // Act - Update livro
        var updateCommand = new UpdateLivroCommand(
            livro.CodL,
            newTitulo,
            newEditora,
            newEdicao,
            newAno,
            new[] { autor.CodAu },
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        var updateResponse = await _client.PutAsJsonAsync($"/api/livros/{livro.CodL}", updateCommand);
        updateResponse.EnsureSuccessStatusCode();

        // Retrieve updated livro
        var retrievedLivro = await GetLivroByIdAsync(livro.CodL);

        // Assert
        retrievedLivro.Should().NotBeNull();
        retrievedLivro!.Titulo.Should().Be(newTitulo);
        retrievedLivro.Editora.Should().Be(newEditora);
        retrievedLivro.Edicao.Should().Be(newEdicao);
        retrievedLivro.AnoPublicacao.Should().Be(newAno);
    }

    /// <summary>
    /// Livro update with new autores replaces old autores

    /// </summary>
    [Fact]
    public async Task LivroUpdateWithNewAutores_ReplacesOldAutores()
    {
        // Arrange
        var autor1 = await CreateAutorAsync("Autor Original");
        var autor2 = await CreateAutorAsync("Autor Novo");

        var livro = await CreateLivroAsync(
            "Livro Para Atualizar",
            "Editora",
            1,
            "2024",
            new[] { autor1.CodAu },
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        // Act - Update with new autor
        var updateCommand = new UpdateLivroCommand(
            livro.CodL,
            livro.Titulo,
            livro.Editora,
            livro.Edicao,
            livro.AnoPublicacao,
            new[] { autor2.CodAu },
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        var updateResponse = await _client.PutAsJsonAsync($"/api/livros/{livro.CodL}", updateCommand);
        updateResponse.EnsureSuccessStatusCode();

        var retrievedLivro = await GetLivroByIdAsync(livro.CodL);

        // Assert
        retrievedLivro.Should().NotBeNull();
        retrievedLivro!.Autores.Should().HaveCount(1);
        retrievedLivro.Autores.First().CodAu.Should().Be(autor2.CodAu);
    }

    /// <summary>
    /// Livro update with new precos replaces old precos

    /// </summary>
    [Fact]
    public async Task LivroUpdateWithNewPrecos_ReplacesOldPrecos()
    {
        // Arrange
        var autor = await CreateAutorAsync("Autor Preco");
        var formasCompra = (await GetFormasCompraAsync()).ToList();
        var forma1 = formasCompra[0];
        var forma2 = formasCompra[1];

        var initialPrecos = new Dictionary<int, decimal> { { forma1.CodFc, 10.00m } };
        var livro = await CreateLivroAsync(
            "Livro Com Preco",
            "Editora",
            1,
            "2024",
            new[] { autor.CodAu },
            Array.Empty<int>(),
            initialPrecos);

        // Act - Update with new preco
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

    #endregion Livro Update Persists Changes

    #region Livro Deletion Cascades to Relationships

    /// <summary>
    /// Livro Deletion Cascades to Relationships

    ///
    /// For any Livro with associated Autores, Assuntos, and Precos, deleting the Livro
    /// should remove the Livro and all its relationship records.
    /// </summary>
    [Property(MaxTest = 20)]
    public async Task LivroDeletionCascadesToRelationships(PositiveInt seed)
    {
        // Arrange - Create livro with relationships
        var autor = await CreateAutorAsync(GenerateValidNome(seed.Get + 200));
        var assunto = await CreateAssuntoAsync(GenerateValidDescricao(seed.Get + 200));
        var formasCompra = (await GetFormasCompraAsync()).ToList();
        var precos = new Dictionary<int, decimal> { { formasCompra[0].CodFc, 15.00m } };

        var livro = await CreateLivroAsync(
            GenerateValidTitulo(seed.Get + 200),
            GenerateValidEditora(seed.Get + 200),
            1,
            "2024",
            new[] { autor.CodAu },
            new[] { assunto.CodAs },
            precos);

        // Act - Delete livro
        var deleteResponse = await _client.DeleteAsync($"/api/livros/{livro.CodL}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Assert - Livro should not exist
        var retrievedLivro = await GetLivroByIdAsync(livro.CodL);
        retrievedLivro.Should().BeNull();

        // Autor and Assunto should still exist (only relationships are deleted)
        var retrievedAutor = await GetAutorByIdAsync(autor.CodAu);
        retrievedAutor.Should().NotBeNull();

        var retrievedAssunto = await GetAssuntoByIdAsync(assunto.CodAs);
        retrievedAssunto.Should().NotBeNull();
    }

    /// <summary>
    /// Deleting livro removes it from database

    /// </summary>
    [Fact]
    public async Task DeletingLivro_RemovesFromDatabase()
    {
        // Arrange
        var autor = await CreateAutorAsync("Autor Para Deletar");
        var livro = await CreateLivroAsync(
            "Livro Para Deletar",
            "Editora",
            1,
            "2024",
            new[] { autor.CodAu },
            Array.Empty<int>(),
            new Dictionary<int, decimal>());

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/livros/{livro.CodL}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/livros/{livro.CodL}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Deleting non-existent livro returns NotFound

    /// </summary>
    [Property(MaxTest = 20)]
    public async Task DeletingNonExistentLivro_ReturnsNotFound(PositiveInt codL)
    {
        // Arrange - Use high ID that doesn't exist
        var nonExistentId = codL.Get + 100000;

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/livros/{nonExistentId}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion Livro Deletion Cascades to Relationships

    #region Autor Update Persists Changes

    /// <summary>
    /// Autor Update Persists Changes

    ///
    /// For any existing Autor and valid update data, updating the Autor and then
    /// retrieving it should return the updated data.
    /// </summary>
    [Property(MaxTest = 20)]
    public async Task AutorUpdatePersistsChanges(PositiveInt seed)
    {
        // Arrange - Create autor
        var originalNome = GenerateValidNome(seed.Get + 300);
        var autor = await CreateAutorAsync(originalNome);

        // Create new nome for update
        var newNome = GenerateValidNome(seed.Get + 400);

        // Act - Update autor
        var updateCommand = new UpdateAutorCommand(autor.CodAu, newNome);
        var updateResponse = await _client.PutAsJsonAsync($"/api/autores/{autor.CodAu}", updateCommand);
        updateResponse.EnsureSuccessStatusCode();

        // Retrieve updated autor
        var retrievedAutor = await GetAutorByIdAsync(autor.CodAu);

        // Assert
        retrievedAutor.Should().NotBeNull();
        retrievedAutor!.Nome.Should().Be(newNome);
        retrievedAutor.CodAu.Should().Be(autor.CodAu);
    }

    /// <summary>
    /// Autor update preserves CodAu

    /// </summary>
    [Fact]
    public async Task AutorUpdate_PreservesCodAu()
    {
        // Arrange
        var autor = await CreateAutorAsync("Nome Original");
        var originalCodAu = autor.CodAu;

        // Act
        var updateCommand = new UpdateAutorCommand(autor.CodAu, "Nome Atualizado");
        var updateResponse = await _client.PutAsJsonAsync($"/api/autores/{autor.CodAu}", updateCommand);
        updateResponse.EnsureSuccessStatusCode();

        var retrievedAutor = await GetAutorByIdAsync(originalCodAu);

        // Assert
        retrievedAutor.Should().NotBeNull();
        retrievedAutor!.CodAu.Should().Be(originalCodAu);
        retrievedAutor.Nome.Should().Be("Nome Atualizado");
    }

    /// <summary>
    /// Updating non-existent autor returns NotFound

    /// </summary>
    [Property(MaxTest = 20)]
    public async Task UpdatingNonExistentAutor_ReturnsNotFound(PositiveInt codAu)
    {
        // Arrange
        var nonExistentId = codAu.Get + 100000;
        var updateCommand = new UpdateAutorCommand(nonExistentId, "Nome Qualquer");

        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/api/autores/{nonExistentId}", updateCommand);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion Autor Update Persists Changes

    #region Assunto Update Persists Changes

    /// <summary>
    /// Assunto Update Persists Changes

    ///
    /// For any existing Assunto and valid update data, updating the Assunto and then
    /// retrieving it should return the updated data.
    /// </summary>
    [Property(MaxTest = 20)]
    public async Task AssuntoUpdatePersistsChanges(PositiveInt seed)
    {
        // Arrange - Create assunto
        var originalDescricao = GenerateValidDescricao(seed.Get + 500);
        var assunto = await CreateAssuntoAsync(originalDescricao);

        // Create new descricao for update
        var newDescricao = GenerateValidDescricao(seed.Get + 600);

        // Act - Update assunto
        var updateCommand = new UpdateAssuntoCommand(assunto.CodAs, newDescricao);
        var updateResponse = await _client.PutAsJsonAsync($"/api/assuntos/{assunto.CodAs}", updateCommand);
        updateResponse.EnsureSuccessStatusCode();

        // Retrieve updated assunto
        var retrievedAssunto = await GetAssuntoByIdAsync(assunto.CodAs);

        // Assert
        retrievedAssunto.Should().NotBeNull();
        retrievedAssunto!.Descricao.Should().Be(newDescricao);
        retrievedAssunto.CodAs.Should().Be(assunto.CodAs);
    }

    /// <summary>
    /// Assunto update preserves CodAs

    /// </summary>
    [Fact]
    public async Task AssuntoUpdate_PreservesCodAs()
    {
        // Arrange
        var assunto = await CreateAssuntoAsync("Desc Original");
        var originalCodAs = assunto.CodAs;

        // Act
        var updateCommand = new UpdateAssuntoCommand(assunto.CodAs, "Desc Atualizada");
        var updateResponse = await _client.PutAsJsonAsync($"/api/assuntos/{assunto.CodAs}", updateCommand);
        updateResponse.EnsureSuccessStatusCode();

        var retrievedAssunto = await GetAssuntoByIdAsync(originalCodAs);

        // Assert
        retrievedAssunto.Should().NotBeNull();
        retrievedAssunto!.CodAs.Should().Be(originalCodAs);
        retrievedAssunto.Descricao.Should().Be("Desc Atualizada");
    }

    /// <summary>
    /// Updating non-existent assunto returns NotFound

    /// </summary>
    [Property(MaxTest = 20)]
    public async Task UpdatingNonExistentAssunto_ReturnsNotFound(PositiveInt codAs)
    {
        // Arrange
        var nonExistentId = codAs.Get + 100000;
        var updateCommand = new UpdateAssuntoCommand(nonExistentId, "Desc Qualquer");

        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/api/assuntos/{nonExistentId}", updateCommand);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion Assunto Update Persists Changes
}
