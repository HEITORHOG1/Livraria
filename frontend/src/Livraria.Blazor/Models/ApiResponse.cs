namespace Livraria.Blazor.Models;

/// <summary>
/// Resposta genérica da API para operações sem retorno de dados.
/// </summary>
public class ApiResponse
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }

    public static ApiResponse Success() => new() { IsSuccess = true };

    public static ApiResponse Failure(string errorMessage) => new() { IsSuccess = false, ErrorMessage = errorMessage };
}

/// <summary>
/// Resposta genérica da API com dados tipados.
/// </summary>
/// <typeparam name="T">Tipo dos dados retornados.</typeparam>
public class ApiResponse<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }

    public static ApiResponse<T> Success(T data) => new() { IsSuccess = true, Data = data };

    public static ApiResponse<T> Failure(string errorMessage) => new() { IsSuccess = false, ErrorMessage = errorMessage };
}

/// <summary>
/// Modelo para deserialização de erros da API.
/// </summary>
public record ErrorResponse(string? Message);