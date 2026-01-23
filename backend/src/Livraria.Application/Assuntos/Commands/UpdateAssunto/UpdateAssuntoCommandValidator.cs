using FluentValidation;

namespace Livraria.Application.Assuntos.Commands.UpdateAssunto;

/// <summary>
/// Validador para o comando de atualização de assunto.
/// </summary>
public class UpdateAssuntoCommandValidator : AbstractValidator<UpdateAssuntoCommand>
{
    public UpdateAssuntoCommandValidator()
    {
        RuleFor(x => x.CodAs)
            .GreaterThan(0).WithMessage("Código do assunto inválido");

        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("Descrição é obrigatória")
            .MaximumLength(20).WithMessage("Descrição deve ter no máximo 20 caracteres");
    }
}