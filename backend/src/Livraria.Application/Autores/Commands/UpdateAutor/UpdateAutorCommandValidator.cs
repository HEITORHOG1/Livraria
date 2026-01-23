using FluentValidation;

namespace Livraria.Application.Autores.Commands.UpdateAutor;

/// <summary>
/// Validador para o comando de atualização de autor.
/// </summary>
public class UpdateAutorCommandValidator : AbstractValidator<UpdateAutorCommand>
{
    public UpdateAutorCommandValidator()
    {
        RuleFor(x => x.CodAu)
            .GreaterThan(0).WithMessage("Código do autor inválido");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(40).WithMessage("Nome deve ter no máximo 40 caracteres");
    }
}