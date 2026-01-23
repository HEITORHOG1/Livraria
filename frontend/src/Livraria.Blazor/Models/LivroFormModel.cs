using System.ComponentModel.DataAnnotations;

namespace Livraria.Blazor.Models;

/// <summary>
/// Modelo de formulário para criação/edição de livros.
/// </summary>
public class LivroFormModel
{
    [Required(ErrorMessage = "Título é obrigatório")]
    [MaxLength(40, ErrorMessage = "Título deve ter no máximo 40 caracteres")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Editora é obrigatória")]
    [MaxLength(40, ErrorMessage = "Editora deve ter no máximo 40 caracteres")]
    public string Editora { get; set; } = string.Empty;

    [Required(ErrorMessage = "Edição é obrigatória")]
    [Range(1, int.MaxValue, ErrorMessage = "Edição deve ser maior que zero")]
    public int Edicao { get; set; } = 1;

    [Required(ErrorMessage = "Ano de publicação é obrigatório")]
    [StringLength(4, MinimumLength = 4, ErrorMessage = "Ano de publicação deve ter exatamente 4 caracteres")]
    public string AnoPublicacao { get; set; } = string.Empty;
}