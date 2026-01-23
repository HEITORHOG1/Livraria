using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using MediatR;

namespace Livraria.Application.Livros.Commands.CreateLivro;

/// <summary>
/// Comando para criar um novo livro.
/// </summary>
public record CreateLivroCommand(
    string Titulo,
    string Editora,
    int Edicao,
    string AnoPublicacao,
    IEnumerable<int> AutoresCodAu,
    IEnumerable<int> AssuntosCodAs,
    IDictionary<int, decimal> Precos
) : IRequest<Result<LivroDto>>;