using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using MediatR;

namespace Livraria.Application.Assuntos.Commands.CreateAssunto;

/// <summary>
/// Comando para criar um novo assunto.
/// </summary>
public record CreateAssuntoCommand(string Descricao) : IRequest<Result<AssuntoDto>>;