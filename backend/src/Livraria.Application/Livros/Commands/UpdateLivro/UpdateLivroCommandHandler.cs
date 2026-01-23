using Livraria.Application.Common.Models;
using Livraria.Application.DTOs;
using Livraria.Application.Mappings;
using Livraria.Domain.Entities;
using Livraria.Domain.Exceptions;
using Livraria.Domain.Interfaces;
using Livraria.Domain.Interfaces.Repositories;
using MediatR;

namespace Livraria.Application.Livros.Commands.UpdateLivro;

/// <summary>
/// Handler para o comando de atualização de livro.
/// </summary>
public class UpdateLivroCommandHandler : IRequestHandler<UpdateLivroCommand, Result<LivroDto>>
{
    private readonly ILivroRepository _livroRepository;
    private readonly IAutorRepository _autorRepository;
    private readonly IAssuntoRepository _assuntoRepository;
    private readonly IFormaCompraRepository _formaCompraRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateLivroCommandHandler(
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

    public async Task<Result<LivroDto>> Handle(UpdateLivroCommand cmd, CancellationToken ct)
    {
        try
        {
            // Buscar livro existente com relacionamentos
            var livro = await _livroRepository.GetByIdWithRelationsAsync(cmd.CodL, ct);
            if (livro is null)
                return Result<LivroDto>.Failure(Error.NotFound($"Livro com código {cmd.CodL} não encontrado"));

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

            // Atualizar dados básicos
            livro.Update(cmd.Titulo, cmd.Editora, cmd.Edicao, cmd.AnoPublicacao);

            // Substituir autores (remove todos e adiciona novos)
            livro.LivroAutores.Clear();
            foreach (var codAu in autoresCodAu)
            {
                livro.LivroAutores.Add(new LivroAutor { Livro_CodL = livro.CodL, Autor_CodAu = codAu });
            }

            // Substituir assuntos (remove todos e adiciona novos)
            livro.LivroAssuntos.Clear();
            foreach (var codAs in assuntosCodAs)
            {
                livro.LivroAssuntos.Add(new LivroAssunto { Livro_CodL = livro.CodL, Assunto_CodAs = codAs });
            }

            // Substituir preços (remove todos e adiciona novos)
            livro.LivroPrecos.Clear();
            foreach (var (codFc, valor) in cmd.Precos)
            {
                LivroPreco.ValidateValor(valor);
                livro.LivroPrecos.Add(new LivroPreco { Livro_CodL = livro.CodL, FormaCompra_CodFc = codFc, Valor = valor });
            }

            _livroRepository.Update(livro);
            await _unitOfWork.SaveChangesAsync(ct);

            // Buscar livro atualizado com relacionamentos
            var livroAtualizado = await _livroRepository.GetByIdWithRelationsAsync(livro.CodL, ct);
            return Result<LivroDto>.Success(livroAtualizado!.ToDto());
        }
        catch (DomainException ex)
        {
            return Result<LivroDto>.Failure(Error.Validation(ex.Message));
        }
    }
}