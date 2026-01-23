using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using MediatR;

namespace Livraria.Application.Autores.Commands.CreateAutor;

/// <summary>
/// Comando para criar um novo autor.
/// </summary>
public record CreateAutorCommand(string Nome) : IRequest<Result<AutorDto>>;