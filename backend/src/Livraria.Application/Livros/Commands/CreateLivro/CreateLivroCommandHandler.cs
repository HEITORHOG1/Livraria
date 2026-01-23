using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using Livraria.Application.Mappings;
using Livraria.Domain.Entities;
using Livraria.Domain.Exceptions;
using Livraria.Domain.Interfaces;
using Livraria.Domain.Interfaces.Repositories;
using MediatR;

namespace Livraria.Application.Livros.Commands.CreateLivro;

/// <summary>
/// Handler para o comando de criação de livro.
/// </summary>
public class CreateLivroCommandHandler : IRequestHandler<CreateLivroCommand, Result<LivroDto>>
{
    private readonly ILivroRepository _livroRepository;
    private readonly IAutorRepository _autorRepository;
    private readonly IAssuntoRepository _assuntoRepository;
    private readonly IFormaCompraRepository _formaCompraRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLivroCommandHandler(
        ILivroRepository livroRepository,
        IAutorRepository autorRepository,
        IAssuntoRepository assuntoRepository,
        IFormaCompraRepository formaCompraRepository,
        IUnitOfWork unitOfWork)
    {
        _livroRepository = livroRepository;
        _autorRepository = autorRepository;
        _assuntoRepository = assuntoRepository;
        _formaCompraRepository = formaCompraRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LivroDto>> Handle(CreateLivroCommand cmd, CancellationToken ct)
    {
        try
        {
            // Validar se autores existem
            var autoresCodAu = cmd.AutoresCodAu.ToList();
            if (autoresCodAu.Count > 0)
            {
                var autoresExistem = await _autorRepository.ExistemAsync(autoresCodAu, ct);
                if (!autoresExistem)
                    return Result<LivroDto>.Failure(Error.Validation("Um ou mais autores não existem"));
            }

            // Validar se assuntos existem
            var assuntosCodAs = cmd.AssuntosCodAs.ToList();
            if (assuntosCodAs.Count > 0)
            {
                var assuntosExistem = await _assuntoRepository.ExistemAsync(assuntosCodAs, ct);
                if (!assuntosExistem)
                    return Result<LivroDto>.Failure(Error.Validation("Um ou mais assuntos não existem"));
            }

            // Validar se formas de compra existem
            var formasCodFc = cmd.Precos.Keys.ToList();
            if (formasCodFc.Count > 0)
            {
                var formasExistem = await _formaCompraRepository.ExistemAsync(formasCodFc, ct);
                if (!formasExistem)
                    return Result<LivroDto>.Failure(Error.Validation("Uma ou mais formas de compra não existem"));
            }

            // Criar entidade
            var livro = Livro.Create(cmd.Titulo, cmd.Editora, cmd.Edicao, cmd.AnoPublicacao);

            // Adicionar autores
            foreach (var codAu in autoresCodAu)
            {
                livro.LivroAutores.Add(new LivroAutor { Autor_CodAu = codAu });
            }

            // Adicionar assuntos
            foreach (var codAs in assuntosCodAs)
            {
                livro.LivroAssuntos.Add(new LivroAssunto { Assunto_CodAs = codAs });
            }

            // Adicionar preços
            foreach (var (codFc, valor) in cmd.Precos)
            {
                LivroPreco.ValidateValor(valor);
                livro.LivroPrecos.Add(new LivroPreco { FormaCompra_CodFc = codFc, Valor = valor });
            }

            await _livroRepository.AddAsync(livro, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            // Buscar livro com relacionamentos para retornar DTO completo
            var livroCompleto = await _livroRepository.GetByIdWithRelationsAsync(livro.CodL, ct);
            return Result<LivroDto>.Success(livroCompleto!.ToDto());
        }
        catch (DomainException ex)
        {
            return Result<LivroDto>.Failure(Error.Validation(ex.Message));
        }
    }
}