using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using Livraria.Application.Mappings;
using Livraria.Domain.Exceptions;
using Livraria.Domain.Interfaces;
using Livraria.Domain.Interfaces.Repositories;
using MediatR;

namespace Livraria.Application.Autores.Commands.UpdateAutor;

/// <summary>
/// Handler para o comando de atualização de autor.
/// </summary>
public class UpdateAutorCommandHandler : IRequestHandler<UpdateAutorCommand, Result<AutorDto>>
{
    private readonly IAutorRepository _autorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAutorCommandHandler(IAutorRepository autorRepository, IUnitOfWork unitOfWork)
    {
        _autorRepository = autorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AutorDto>> Handle(UpdateAutorCommand cmd, CancellationToken ct)
    {
        try
        {
            var autor = await _autorRepository.GetByIdAsync(cmd.CodAu, ct);
            if (autor is null)
                return Result<AutorDto>.Failure(Error.NotFound($"Autor com código {cmd.CodAu} não encontrado"));

            autor.Update(cmd.Nome);

            _autorRepository.Update(autor);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<AutorDto>.Success(autor.ToDto());
        }
        catch (DomainException ex)
        {
            return Result<AutorDto>.Failure(Error.Validation(ex.Message));
        }
    }
}