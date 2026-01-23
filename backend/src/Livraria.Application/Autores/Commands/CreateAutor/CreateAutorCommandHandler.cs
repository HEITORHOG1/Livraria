using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using Livraria.Application.Mappings;
using Livraria.Domain.Entities;
using Livraria.Domain.Exceptions;
using Livraria.Domain.Interfaces;
using Livraria.Domain.Interfaces.Repositories;
using MediatR;

namespace Livraria.Application.Autores.Commands.CreateAutor;

/// <summary>
/// Handler para o comando de criação de autor.
/// </summary>
public class CreateAutorCommandHandler : IRequestHandler<CreateAutorCommand, Result<AutorDto>>
{
    private readonly IAutorRepository _autorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAutorCommandHandler(IAutorRepository autorRepository, IUnitOfWork unitOfWork)
    {
        _autorRepository = autorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AutorDto>> Handle(CreateAutorCommand cmd, CancellationToken ct)
    {
        try
        {
            var autor = Autor.Create(cmd.Nome);

            await _autorRepository.AddAsync(autor, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return Result<AutorDto>.Success(autor.ToDto());
        }
        catch (DomainException ex)
        {
            return Result<AutorDto>.Failure(Error.Validation(ex.Message));
        }
    }
}