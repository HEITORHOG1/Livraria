using Livraria.Application.Common.Models;
using MediatR;

namespace Livraria.Application.Autores.Commands.DeleteAutor;

/// <summary>
/// Comando para excluir um autor.
/// </summary>
public record DeleteAutorCommand(int CodAu) : IRequest<Result>;