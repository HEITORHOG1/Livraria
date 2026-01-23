using FluentValidation;

namespace Livraria.Application.Autores.Commands.CreateAutor;

/// <summary>
/// Validador para o comando de criação de autor.
/// </summary>
public class CreateAutorCommandValidator : AbstractValidator<CreateAutorCommand>
{
    public CreateAutorCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(40).WithMessage("Nome deve ter no máximo 40 caracteres");
    }
}