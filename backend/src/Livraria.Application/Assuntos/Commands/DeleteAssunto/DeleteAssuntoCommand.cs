using Livraria.Application.Common.Models;
using MediatR;

namespace Livraria.Application.Assuntos.Commands.DeleteAssunto;

/// <summary>
/// Comando para excluir um assunto.
/// </summary>
public record DeleteAssuntoCommand(int CodAs) : IRequest<Result>;