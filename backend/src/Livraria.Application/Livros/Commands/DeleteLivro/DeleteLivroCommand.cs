using Livraria.Application.Common.Models;
using MediatR;

namespace Livraria.Application.Livros.Commands.DeleteLivro;

/// <summary>
/// Comando para excluir um livro.
/// </summary>
public record DeleteLivroCommand(int CodL) : IRequest<Result>;