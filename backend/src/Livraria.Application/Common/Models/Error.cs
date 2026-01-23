namespace Livraria.Application.Common.Models;

/// <summary>
/// Representa um erro no sistema com código e mensagem.
/// </summary>
public record Error(string Code, string Message)
{
    public static Error Validation(string message) => new("VALIDATION_ERROR", message);
    public static Error NotFound(string message) => new("NOT_FOUND", message);
    public static Error Conflict(string message) => new("CONFLICT", message);
    public static Error Database(string message) => new("DATABASE_ERROR", message);
}