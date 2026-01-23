using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using Livraria.Application.Mappings;
using Livraria.Domain.Entities;
using Livraria.Domain.Exceptions;
using Livraria.Domain.Interfaces;
using Livraria.Domain.Interfaces.Repositories;
using MediatR;

namespace Livraria.Application.Assuntos.Commands.CreateAssunto;

/// <summary>
/// Handler para o comando de criação de assunto.
/// </summary>
public class CreateAssuntoCommandHandler : IRequestHandler<CreateAssuntoCommand, Result<AssuntoDto>>
{
    private readonly IAssuntoRepository _assuntoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAssuntoCommandHandler(IAssuntoRepository assuntoRepository, IUnitOfWork unitOfWork)
    {
        _assuntoRepository = assuntoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AssuntoDto>> Handle(CreateAssuntoCommand cmd, CancellationToken ct)
    {
        try
        {
            var assunto = Assunto.Create(cmd.Descricao);

            await _assuntoRepository.AddAsync(assunto, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<AssuntoDto>.Success(assunto.ToDto());
        }
        catch (DomainException ex)
        {
            return Result<AssuntoDto>.Failure(Error.Validation(ex.Message));
        }
    }
}