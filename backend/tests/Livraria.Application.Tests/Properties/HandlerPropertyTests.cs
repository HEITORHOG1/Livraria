using FsCheck;
using FsCheck.Xunit;
using Livraria.Application.Assuntos.Commands.CreateAssunto;
using Livraria.Application.Autores.Commands.CreateAutor;
using Livraria.Application.Livros.Commands.CreateLivro;
using Livraria.Domain.Entities;
using Livraria.Domain.Interfaces;
using Livraria.Domain.Interfaces.Repositories;
using Moq;

namespace Livraria.Application.Tests.Properties;

/// <summary>
/// Property tests for CQRS handlers.
///
/// Valid Livro Creation Persists Data
/// Valid Autor Creation Persists Data
/// Valid Assunto Creation Persists Data
///

/// </summary>
public class HandlerPropertyTests
{
    #region Valid Livro Creation Persists Data

    /// <summary>
    /// Valid Livro Creation Persists Data
    ///
    /// For any valid book data (Titulo ≤ 40 chars non-empty, Editora ≤ 40 chars non-empty,
    /// Edicao > 0, AnoPublicacao = 4 chars), creating a Livro through the handler should
    /// succeed and return the same data that was submitted.
    ///

    /// </summary>
    [Property(MaxTest = 100)]
    public bool ValidLivroCreation_ShouldPersistData(PositiveInt tituloLen, PositiveInt editoraLen, PositiveInt edicao, PositiveInt ano)
    {
        // Arrange - Generate valid data
        var titulo = new string('a', (tituloLen.Get % 40) + 1);
        var editora = new string('b', (editoraLen.Get % 40) + 1);
        var edicaoVal = edicao.Get;
        var anoVal = ((ano.Get % 9000) + 1000).ToString(); // 1000-9999

        Livro? capturedLivro = null;

        var mockLivroRepo = new Mock<ILivroRepository>();
        mockLivroRepo.Setup(r => r.AddAsync(It.IsAny<Livro>(), It.IsAny<CancellationToken>()))
            .Callback<Livro, CancellationToken>((l, _) => capturedLivro = l)
            .Returns(Task.CompletedTask);
        mockLivroRepo.Setup(r => r.GetByIdWithRelationsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => capturedLivro);

        var mockAutorRepo = new Mock<IAutorRepository>();
        mockAutorRepo.Setup(r => r.ExistemAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var mockAssuntoRepo = new Mock<IAssuntoRepository>();
        mockAssuntoRepo.Setup(r => r.ExistemAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var mockFormaCompraRepo = new Mock<IFormaCompraRepository>();
        mockFormaCompraRepo.Setup(r => r.ExistemAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateLivroCommandHandler(
            mockLivroRepo.Object,
            mockAutorRepo.Object,
            mockAssuntoRepo.Object,
            mockFormaCompraRepo.Object,
            mockUnitOfWork.Object);

        var command = new CreateLivroCommand(
            titulo, editora, edicaoVal, anoVal,
            [1], // At least one author
            [], // No subjects required
            new Dictionary<int, decimal>()); // No prices required

        // Act
        var result = handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

        // Assert
        return result.IsSuccess
            && capturedLivro != null
            && capturedLivro.Titulo == titulo
            && capturedLivro.Editora == editora
            && capturedLivro.Edicao == edicaoVal
            && capturedLivro.AnoPublicacao == anoVal;
    }

    #endregion Valid Livro Creation Persists Data

    #region Valid Autor Creation Persists Data

    /// <summary>
    /// Valid Autor Creation Persists Data
    ///
    /// For any valid author data (Nome ≤ 40 chars non-empty), creating an Autor through
    /// the handler should succeed and return the same data that was submitted.
    ///

    /// </summary>
    [Property(MaxTest = 100)]
    public bool ValidAutorCreation_ShouldPersistData(PositiveInt nomeLen)
    {
        // Arrange - Generate valid data
        var nome = new string('a', (nomeLen.Get % 40) + 1);

        Autor? capturedAutor = null;

        var mockAutorRepo = new Mock<IAutorRepository>();
        mockAutorRepo.Setup(r => r.AddAsync(It.IsAny<Autor>(), It.IsAny<CancellationToken>()))
            .Callback<Autor, CancellationToken>((a, _) => capturedAutor = a)
            .Returns(Task.CompletedTask);

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateAutorCommandHandler(mockAutorRepo.Object, mockUnitOfWork.Object);
        var command = new CreateAutorCommand(nome);

        // Act
        var result = handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

        // Assert
        return result.IsSuccess
            && capturedAutor != null
            && capturedAutor.Nome == nome;
    }

    #endregion Valid Autor Creation Persists Data

    #region Valid Assunto Creation Persists Data

    /// <summary>
    /// Valid Assunto Creation Persists Data
    ///
    /// For any valid subject data (Descricao ≤ 20 chars non-empty), creating an Assunto
    /// through the handler should succeed and return the same data that was submitted.
    ///

    /// </summary>
    [Property(MaxTest = 100)]
    public bool ValidAssuntoCreation_ShouldPersistData(PositiveInt descricaoLen)
    {
        // Arrange - Generate valid data
        var descricao = new string('a', (descricaoLen.Get % 20) + 1);

        Assunto? capturedAssunto = null;

        var mockAssuntoRepo = new Mock<IAssuntoRepository>();
        mockAssuntoRepo.Setup(r => r.AddAsync(It.IsAny<Assunto>(), It.IsAny<CancellationToken>()))
            .Callback<Assunto, CancellationToken>((a, _) => capturedAssunto = a)
            .Returns(Task.CompletedTask);

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateAssuntoCommandHandler(mockAssuntoRepo.Object, mockUnitOfWork.Object);
        var command = new CreateAssuntoCommand(descricao);

        // Act
        var result = handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();

        // Assert
        return result.IsSuccess
            && capturedAssunto != null
            && capturedAssunto.Descricao == descricao;
    }

    #endregion Valid Assunto Creation Persists Data
}
