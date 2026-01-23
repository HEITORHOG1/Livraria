using FluentValidation;

namespace Livraria.Application.Livros.Commands.UpdateLivro;

/// <summary>
/// Validador para o comando de atualização de livro.
/// </summary>
public class UpdateLivroCommandValidator : AbstractValidator<UpdateLivroCommand>
{
    public UpdateLivroCommandValidator()
    {
        RuleFor(x => x.CodL)
            .GreaterThan(0).WithMessage("Código do livro inválido");

        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("Título é obrigatório")
            .MaximumLength(40).WithMessage("Título deve ter no máximo 40 caracteres");

        RuleFor(x => x.Editora)
            .NotEmpty().WithMessage("Editora é obrigatória")
            .MaximumLength(40).WithMessage("Editora deve ter no máximo 40 caracteres");

        RuleFor(x => x.Edicao)
            .GreaterThan(0).WithMessage("Edição deve ser maior que zero");

        RuleFor(x => x.AnoPublicacao)
            .NotEmpty().WithMessage("Ano de publicação é obrigatório")
            .Length(4).WithMessage("Ano de publicação deve ter exatamente 4 caracteres");

        RuleFor(x => x.AutoresCodAu)
            .NotEmpty().WithMessage("Pelo menos um autor é obrigatório");

        RuleForEach(x => x.Precos.Values)
            .GreaterThanOrEqualTo(0).WithMessage("Preço não pode ser negativo");
    }
}