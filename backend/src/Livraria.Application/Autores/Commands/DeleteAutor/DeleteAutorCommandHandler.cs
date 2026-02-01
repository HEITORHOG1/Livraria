using Livraria.Application.Common.Models;
using Livraria.Domain.Interfaces;
using Livraria.Domain.Interfaces.Repositories;
using MediatR;

namespace Livraria.Application.Autores.Commands.DeleteAutor;

/// <summary>
/// Handler para o comando de exclusão de autor.
/// </summary>
public class DeleteAutorCommandHandler : IRequestHandler<DeleteAutorCommand, Result>
{
    private readonly IAutorRepository _autorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAutorCommandHandler(IAutorRepository autorRepository, IUnitOfWork unitOfWork)
    {
        _autorRepository = autorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteAutorCommand cmd, CancellationToken ct)
    {
        var autor = await _autorRepository.GetByIdAsync(cmd.CodAu, ct);
        if (autor is null)
            return Result.Failure(Error.NotFound($"Autor com código {cmd.CodAu} não encontrado"));

        var hasLivros = await _autorRepository.HasLivrosVinculadosAsync(cmd.CodAu, ct);
        if (hasLivros)
            return Result.Failure(Error.Conflict($"Não é possível excluir o autor '{autor.Nome}' pois existem livros vinculados"));

        _autorRepository.Delete(autor);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}