using Livraria.Application.Common.Models;
using Livraria.Domain.Interfaces;
using Livraria.Domain.Interfaces.Repositories;
using MediatR;

namespace Livraria.Application.Assuntos.Commands.DeleteAssunto;

/// <summary>
/// Handler para o comando de exclusão de assunto.
/// </summary>
public class DeleteAssuntoCommandHandler : IRequestHandler<DeleteAssuntoCommand, Result>
{
    private readonly IAssuntoRepository _assuntoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAssuntoCommandHandler(IAssuntoRepository assuntoRepository, IUnitOfWork unitOfWork)
    {
        _assuntoRepository = assuntoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteAssuntoCommand cmd, CancellationToken ct)
    {
        var assunto = await _assuntoRepository.GetByIdAsync(cmd.CodAs, ct);
        if (assunto is null)
            return Result.Failure(Error.NotFound($"Assunto com código {cmd.CodAs} não encontrado"));

        var hasLivros = await _assuntoRepository.HasLivrosVinculadosAsync(cmd.CodAs, ct);
        if (hasLivros)
            return Result.Failure(Error.Conflict($"Não é possível excluir o assunto '{assunto.Descricao}' pois existem livros vinculados"));

        _assuntoRepository.Delete(assunto);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
}