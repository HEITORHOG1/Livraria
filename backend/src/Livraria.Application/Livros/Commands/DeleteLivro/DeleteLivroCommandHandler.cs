using Livraria.Application.Common.Models;
using Livraria.Domain.Interfaces;
using Livraria.Domain.Interfaces.Repositories;
using MediatR;

namespace Livraria.Application.Livros.Commands.DeleteLivro;

/// <summary>
/// Handler para o comando de exclusão de livro.
/// </summary>
public class DeleteLivroCommandHandler : IRequestHandler<DeleteLivroCommand, Result>
{
    private readonly ILivroRepository _livroRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteLivroCommandHandler(ILivroRepository livroRepository, IUnitOfWork unitOfWork)
    {
        _livroRepository = livroRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteLivroCommand cmd, CancellationToken ct)
    {
        var livro = await _livroRepository.GetByIdAsync(cmd.CodL, ct);
        if (livro is null)
            return Result.Failure(Error.NotFound($"Livro com código {cmd.CodL} não encontrado"));

        _livroRepository.Delete(livro);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}