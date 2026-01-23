using FsCheck;
using FsCheck.Xunit;
using Livraria.Domain.Entities;
using Livraria.Infrastructure.Data;
using Livraria.Infrastructure.Data.Seed;
using Livraria.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Livraria.Integration.Tests.Properties;

/// <summary>
/// Property-based tests para a camada Infrastructure.
/// </summary>
public class InfrastructurePropertyTests
{
    private static ApplicationDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    #region Seed Idempotency

    /// <summary>
    /// Seed Idempotency

    /// Running seed multiple times should not create duplicate FormasCompra.
    /// </summary>
    [Property(MaxTest = 10)]
    public bool SeedIdempotency_MultipleSeedsDoNotCreateDuplicates(PositiveInt seedCount)
    {
        // Arrange
        var count = (seedCount.Get % 5) + 2; // 2-6 times
        var dbName = $"SeedIdempotency_{Guid.NewGuid()}";
        using var context = CreateInMemoryContext(dbName);
        var logger = new Mock<ILogger<DatabaseSeeder>>();
        var seeder = new DatabaseSeeder(context, logger.Object);

        // Act - Run seed multiple times
        for (int i = 0; i < count; i++)
        {
            seeder.SeedAsync().GetAwaiter().GetResult();
        }

        // Assert - Should have exactly 4 FormasCompra (Balcão, Self-Service, Internet, Evento)
        var formasCompra = context.FormasCompra.ToList();
        return formasCompra.Count == 4
            && formasCompra.Select(f => f.Descricao).Distinct().Count() == 4;
    }

    /// <summary>
    /// Seed creates expected FormasCompra

    /// </summary>
    [Fact]
    public void Seed_CreatesExpectedFormasCompra()
    {
        // Arrange
        var dbName = $"SeedExpected_{Guid.NewGuid()}";
        using var context = CreateInMemoryContext(dbName);
        var logger = new Mock<ILogger<DatabaseSeeder>>();
        var seeder = new DatabaseSeeder(context, logger.Object);

        // Act
        seeder.SeedAsync().GetAwaiter().GetResult();

        // Assert
        var formasCompra = context.FormasCompra.ToList();
        var descricoes = formasCompra.Select(f => f.Descricao).OrderBy(d => d).ToList();

        Assert.Equal(4, formasCompra.Count);
        Assert.Contains("Balcão", descricoes);
        Assert.Contains("Self-Service", descricoes);
        Assert.Contains("Internet", descricoes);
        Assert.Contains("Evento", descricoes);
    }

    #endregion Seed Idempotency

    #region GetAll Returns All Entities

    /// <summary>
    /// GetAll Returns All Entities (Autores)

    /// GetAllAsync should return all entities that were added.
    /// </summary>
    [Property(MaxTest = 20)]
    public bool GetAllAutores_ReturnsAllAddedEntities(PositiveInt count)
    {
        // Arrange
        var entityCount = (count.Get % 5) + 1; // 1-5 entities
        var dbName = $"GetAllAutores_{Guid.NewGuid()}";
        using var context = CreateInMemoryContext(dbName);
        var repository = new AutorRepository(context);

        // Act - Add autores
        for (int i = 0; i < entityCount; i++)
        {
            var autor = Autor.Create($"Autor {i}");
            repository.AddAsync(autor).GetAwaiter().GetResult();
        }
        context.SaveChanges();

        // Assert - GetAll should return all added entities
        var result = repository.GetAllAsync().GetAwaiter().GetResult().ToList();
        return result.Count == entityCount;
    }

    /// <summary>
    /// GetAll Returns All Entities (Assuntos)

    /// </summary>
    [Property(MaxTest = 20)]
    public bool GetAllAssuntos_ReturnsAllAddedEntities(PositiveInt count)
    {
        // Arrange
        var entityCount = (count.Get % 5) + 1; // 1-5 entities
        var dbName = $"GetAllAssuntos_{Guid.NewGuid()}";
        using var context = CreateInMemoryContext(dbName);
        var repository = new AssuntoRepository(context);

        // Act - Add assuntos
        for (int i = 0; i < entityCount; i++)
        {
            var assunto = Assunto.Create($"Assunto {i % 20}".Substring(0, Math.Min(20, $"Assunto {i}".Length)));
            repository.AddAsync(assunto).GetAwaiter().GetResult();
        }
        context.SaveChanges();

        // Assert - GetAll should return all added entities
        var result = repository.GetAllAsync().GetAwaiter().GetResult().ToList();
        return result.Count == entityCount;
    }

    /// <summary>
    /// GetAll Returns All Entities (Livros)

    /// </summary>
    [Property(MaxTest = 20)]
    public bool GetAllLivros_ReturnsAllAddedEntities(PositiveInt count)
    {
        // Arrange
        var entityCount = (count.Get % 5) + 1; // 1-5 entities
        var dbName = $"GetAllLivros_{Guid.NewGuid()}";
        using var context = CreateInMemoryContext(dbName);
        var repository = new LivroRepository(context);

        // Act - Add livros
        for (int i = 0; i < entityCount; i++)
        {
            var livro = Livro.Create($"Livro {i}", "Editora", 1, "2024");
            repository.AddAsync(livro).GetAwaiter().GetResult();
        }
        context.SaveChanges();

        // Assert - GetAll should return all added entities
        var result = repository.GetAllAsync().GetAwaiter().GetResult().ToList();
        return result.Count == entityCount;
    }

    /// <summary>
    /// ExistemAsync returns true for existing entities

    /// </summary>
    [Property(MaxTest = 20)]
    public bool AutorRepository_ExistemAsync_ReturnsTrueForExistingEntities(PositiveInt count)
    {
        // Arrange
        var entityCount = (count.Get % 5) + 1; // 1-5 entities
        var dbName = $"ExistemAutores_{Guid.NewGuid()}";
        using var context = CreateInMemoryContext(dbName);
        var repository = new AutorRepository(context);

        // Act - Add autores
        var addedIds = new List<int>();
        for (int i = 0; i < entityCount; i++)
        {
            var autor = Autor.Create($"Autor {i}");
            repository.AddAsync(autor).GetAwaiter().GetResult();
            context.SaveChanges();
            addedIds.Add(autor.CodAu);
        }

        // Assert - ExistemAsync should return true for all added IDs
        var result = repository.ExistemAsync(addedIds).GetAwaiter().GetResult();
        return result;
    }

    /// <summary>
    /// ExistemAsync returns false for non-existing entities

    /// </summary>
    [Property(MaxTest = 20)]
    public bool AutorRepository_ExistemAsync_ReturnsFalseForNonExistingEntities(PositiveInt id)
    {
        // Arrange
        var dbName = $"ExistemAutoresNot_{Guid.NewGuid()}";
        using var context = CreateInMemoryContext(dbName);
        var repository = new AutorRepository(context);

        // Act - Check for non-existing IDs
        var nonExistingIds = new List<int> { id.Get + 1000 }; // IDs that don't exist

        // Assert - ExistemAsync should return false
        var result = repository.ExistemAsync(nonExistingIds).GetAwaiter().GetResult();
        return !result;
    }

    #endregion GetAll Returns All Entities
}
