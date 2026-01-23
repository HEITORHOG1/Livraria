using FsCheck;
using FsCheck.Xunit;
using Livraria.Application.Common.Interfaces;
using Livraria.Application.DTOs;
using Livraria.Application.Relatorios.Queries.GetRelatorioLivrosPorAutor;
using Moq;

namespace Livraria.Integration.Tests.Properties;

/// <summary>
/// Property-based tests para relatórios.
/// Property 15: Report Data Ordering

/// </summary>
public class RelatorioPropertyTests
{
    private static readonly string[] AutorNames = { "Ana", "Bruno", "Carlos", "Diana", "Eduardo", "Fernanda", "Gabriel", "Helena" };
    private static readonly string[] Titulos = { "Aventura", "Busca", "Caminho", "Destino", "Esperança", "Futuro", "Guerra", "Harmonia" };
    private static readonly string[] Editoras = { "Editora A", "Editora B", "Editora C" };
    private static readonly string?[] Assuntos = { "Ficção", "Romance", "Aventura", "Ciência", null };
    private static readonly string[] Anos = { "2020", "2021", "2022", "2023", "2024" };



    /// <summary>
    /// For any set of books with authors, the report data should be ordered first by Autor name (alphabetically),
    /// then by Titulo (alphabetically) within each author.

    /// For any set of books with authors, the report data should be ordered first by Autor name (alphabetically),
    /// then by Titulo (alphabetically) within each author.
    /// </summary>
    [Property(MaxTest = 100)]
    public bool ReportDataOrdering_ShouldOrderByAutorThenByTitulo(PositiveInt seed)
    {
        // Arrange - Generate random data
        var random = new Random(seed.Get);
        var count = random.Next(0, 16);
        var dados = GenerateRandomData(random, count);

        var mockService = new Mock<IRelatorioService>();
        mockService.Setup(s => s.GetDadosRelatorioAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(dados);

        var handler = new GetRelatorioLivrosPorAutorQueryHandler(mockService.Object);

        // Act
        var result = handler.Handle(new GetRelatorioLivrosPorAutorQuery(), CancellationToken.None)
            .GetAwaiter().GetResult();

        // Assert
        if (!result.IsSuccess || !result.Value!.Any())
            return true; // Empty or failed results are trivially ordered

        var resultList = result.Value!.ToList();

        // Verify ordering: first by Autor, then by Titulo
        for (int i = 1; i < resultList.Count; i++)
        {
            var prev = resultList[i - 1];
            var curr = resultList[i];

            var autorComparison = string.Compare(prev.Autor, curr.Autor, StringComparison.Ordinal);

            if (autorComparison > 0)
            {
                // Previous autor should not be greater than current
                return false;
            }

            if (autorComparison == 0)
            {
                // Same autor, titulo should be ordered
                var tituloComparison = string.Compare(prev.Titulo, curr.Titulo, StringComparison.Ordinal);
                if (tituloComparison > 0)
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Verify all items are present after ordering

    /// </summary>
    [Property(MaxTest = 100)]
    public bool ReportDataOrdering_ShouldPreserveAllItems(PositiveInt seed)
    {
        // Arrange - Generate random data
        var random = new Random(seed.Get);
        var count = random.Next(0, 16);
        var dados = GenerateRandomData(random, count);

        var mockService = new Mock<IRelatorioService>();
        mockService.Setup(s => s.GetDadosRelatorioAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(dados);

        var handler = new GetRelatorioLivrosPorAutorQueryHandler(mockService.Object);

        // Act
        var result = handler.Handle(new GetRelatorioLivrosPorAutorQuery(), CancellationToken.None)
            .GetAwaiter().GetResult();

        // Assert
        if (!result.IsSuccess)
            return true;

        var resultList = result.Value!.ToList();
        var inputList = dados.ToList();

        // Same count
        if (resultList.Count != inputList.Count)
            return false;

        // All items present (by CodL which should be unique)
        var inputCodLs = inputList.Select(d => d.CodL).OrderBy(x => x).ToList();
        var resultCodLs = resultList.Select(d => d.CodL).OrderBy(x => x).ToList();

        return inputCodLs.SequenceEqual(resultCodLs);
    }

    /// <summary>
    /// Reverse ordered input should be correctly sorted

    /// </summary>
    [Property(MaxTest = 100)]
    public bool ReportDataOrdering_ReverseOrderedInput_ShouldBeSorted(PositiveInt seed)
    {
        // Arrange - Generate data in reverse order
        var random = new Random(seed.Get);
        var count = random.Next(2, 10);
        var dados = GenerateRandomData(random, count)
            .OrderByDescending(d => d.Autor)
            .ThenByDescending(d => d.Titulo)
            .ToList();

        var mockService = new Mock<IRelatorioService>();
        mockService.Setup(s => s.GetDadosRelatorioAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(dados);

        var handler = new GetRelatorioLivrosPorAutorQueryHandler(mockService.Object);

        // Act
        var result = handler.Handle(new GetRelatorioLivrosPorAutorQuery(), CancellationToken.None)
            .GetAwaiter().GetResult();

        // Assert
        if (!result.IsSuccess || !result.Value!.Any())
            return true;

        var resultList = result.Value!.ToList();

        // Verify ordering: first by Autor, then by Titulo
        for (int i = 1; i < resultList.Count; i++)
        {
            var prev = resultList[i - 1];
            var curr = resultList[i];

            var autorComparison = string.Compare(prev.Autor, curr.Autor, StringComparison.Ordinal);

            if (autorComparison > 0)
                return false;

            if (autorComparison == 0)
            {
                var tituloComparison = string.Compare(prev.Titulo, curr.Titulo, StringComparison.Ordinal);
                if (tituloComparison > 0)
                    return false;
            }
        }

        return true;
    }



    #region Unit Tests

    /// <summary>
    /// Empty data should return empty result

    /// </summary>
    [Fact]
    public async Task ReportDataOrdering_EmptyData_ShouldReturnEmptyResult()
    {
        // Arrange
        var mockService = new Mock<IRelatorioService>();
        mockService.Setup(s => s.GetDadosRelatorioAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<RelatorioLivroDto>());

        var handler = new GetRelatorioLivrosPorAutorQueryHandler(mockService.Object);

        // Act
        var result = await handler.Handle(new GetRelatorioLivrosPorAutorQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    /// <summary>
    /// Single item should return that item

    /// </summary>
    [Fact]
    public async Task ReportDataOrdering_SingleItem_ShouldReturnThatItem()
    {
        // Arrange
        var singleItem = new RelatorioLivroDto(1, "Autor A", 1, "Titulo A", "Editora", 1, "2024", "Assunto");
        var mockService = new Mock<IRelatorioService>();
        mockService.Setup(s => s.GetDadosRelatorioAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { singleItem });

        var handler = new GetRelatorioLivrosPorAutorQueryHandler(mockService.Object);

        // Act
        var result = await handler.Handle(new GetRelatorioLivrosPorAutorQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal(singleItem, result.Value!.First());
    }

    /// <summary>
    /// Multiple authors should be sorted alphabetically

    /// </summary>
    [Fact]
    public async Task ReportDataOrdering_MultipleAuthors_ShouldSortAlphabetically()
    {
        // Arrange
        var dados = new[]
        {
            new RelatorioLivroDto(3, "Carlos", 3, "Livro C", "Editora", 1, "2024", null),
            new RelatorioLivroDto(1, "Ana", 1, "Livro A", "Editora", 1, "2024", null),
            new RelatorioLivroDto(2, "Bruno", 2, "Livro B", "Editora", 1, "2024", null),
        };

        var mockService = new Mock<IRelatorioService>();
        mockService.Setup(s => s.GetDadosRelatorioAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(dados);

        var handler = new GetRelatorioLivrosPorAutorQueryHandler(mockService.Object);

        // Act
        var result = await handler.Handle(new GetRelatorioLivrosPorAutorQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var resultList = result.Value!.ToList();
        Assert.Equal("Ana", resultList[0].Autor);
        Assert.Equal("Bruno", resultList[1].Autor);
        Assert.Equal("Carlos", resultList[2].Autor);
    }

    /// <summary>
    /// Same author, multiple books should sort by title

    /// </summary>
    [Fact]
    public async Task ReportDataOrdering_SameAuthor_ShouldSortByTitle()
    {
        // Arrange
        var dados = new[]
        {
            new RelatorioLivroDto(1, "Ana", 3, "Zebra", "Editora", 1, "2024", null),
            new RelatorioLivroDto(1, "Ana", 1, "Aventura", "Editora", 1, "2024", null),
            new RelatorioLivroDto(1, "Ana", 2, "Mistério", "Editora", 1, "2024", null),
        };

        var mockService = new Mock<IRelatorioService>();
        mockService.Setup(s => s.GetDadosRelatorioAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(dados);

        var handler = new GetRelatorioLivrosPorAutorQueryHandler(mockService.Object);

        // Act
        var result = await handler.Handle(new GetRelatorioLivrosPorAutorQuery(), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var resultList = result.Value!.ToList();
        Assert.Equal("Aventura", resultList[0].Titulo);
        Assert.Equal("Mistério", resultList[1].Titulo);
        Assert.Equal("Zebra", resultList[2].Titulo);
    }

    #endregion Unit Tests

    #region Helpers

    private static List<RelatorioLivroDto> GenerateRandomData(Random random, int count)
    {
        var result = new List<RelatorioLivroDto>();
        for (int i = 0; i < count; i++)
        {
            var codAu = random.Next(1, 101);
            var autor = AutorNames[random.Next(AutorNames.Length)];
            var codL = i + 1; // Unique CodL
            var titulo = Titulos[random.Next(Titulos.Length)];
            var editora = Editoras[random.Next(Editoras.Length)];
            var edicao = random.Next(1, 11);
            var ano = Anos[random.Next(Anos.Length)];
            var assunto = Assuntos[random.Next(Assuntos.Length)];

            result.Add(new RelatorioLivroDto(codAu, autor, codL, titulo, editora, edicao, ano, assunto));
        }
        return result;
    }

    #endregion Helpers
}