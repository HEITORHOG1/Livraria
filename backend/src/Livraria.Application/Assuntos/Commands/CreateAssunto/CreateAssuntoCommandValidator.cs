using FluentValidation;

namespace Livraria.Application.Assuntos.Commands.CreateAssunto;

/// <summary>
/// Validador para o comando de criação de assunto.
/// </summary>
public class CreateAssuntoCommandValidator : AbstractValidator<CreateAssuntoCommand>
{
    public CreateAssuntoCommandValidator()
    {
        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("Descrição é obrigatória")
            .MaximumLength(20).WithMessage("Descrição deve ter no máximo 20 caracteres");
    }
}