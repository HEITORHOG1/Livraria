using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using MediatR;

namespace Livraria.Application.Assuntos.Commands.UpdateAssunto;

/// <summary>
/// Comando para atualizar um assunto existente.
/// </summary>
public record UpdateAssuntoCommand(int CodAs, string Descricao) : IRequest<Result<AssuntoDto>>;