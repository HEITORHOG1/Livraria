using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using MediatR;

namespace Livraria.Application.Autores.Commands.UpdateAutor;

/// <summary>
/// Comando para atualizar um autor existente.
/// </summary>
public record UpdateAutorCommand(int CodAu, string Nome) : IRequest<Result<AutorDto>>;