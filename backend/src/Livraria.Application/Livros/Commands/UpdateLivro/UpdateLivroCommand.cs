using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using MediatR;

namespace Livraria.Application.Livros.Commands.UpdateLivro;

/// <summary>
/// Comando para atualizar um livro existente.
/// </summary>
public record UpdateLivroCommand(
    int CodL,
    string Titulo,
    string Editora,
    int Edicao,
    string AnoPublicacao,
    IEnumerable<int> AutoresCodAu,
    IEnumerable<int> AssuntosCodAs,
    IDictionary<int, decimal> Precos
) : IRequest<Result<LivroDto>>;