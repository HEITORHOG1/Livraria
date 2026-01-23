using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using Livraria.Application.Mappings;
using Livraria.Domain.Exceptions;
using Livraria.Domain.Interfaces;
using Livraria.Domain.Interfaces.Repositories;
using MediatR;

namespace Livraria.Application.Assuntos.Commands.UpdateAssunto;

/// <summary>
/// Handler para o comando de atualização de assunto.
/// </summary>
public class UpdateAssuntoCommandHandler : IRequestHandler<UpdateAssuntoCommand, Result<AssuntoDto>>
{
    private readonly IAssuntoRepository _assuntoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAssuntoCommandHandler(IAssuntoRepository assuntoRepository, IUnitOfWork unitOfWork)
    {
        _assuntoRepository = assuntoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AssuntoDto>> Handle(UpdateAssuntoCommand cmd, CancellationToken ct)
    {
        try
        {
            var assunto = await _assuntoRepository.GetByIdAsync(cmd.CodAs, ct);
            if (assunto is null)
                return Result<AssuntoDto>.Failure(Error.NotFound($"Assunto com código {cmd.CodAs} não encontrado"));

            assunto.Update(cmd.Descricao);

            _assuntoRepository.Update(assunto);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<AssuntoDto>.Success(assunto.ToDto());
        }
        catch (DomainException ex)
        {
            return Result<AssuntoDto>.Failure(Error.Validation(ex.Message));
        }
    }
}