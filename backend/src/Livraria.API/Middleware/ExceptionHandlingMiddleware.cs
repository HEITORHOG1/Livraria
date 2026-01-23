using FluentValidation;
using Livraria.Domain.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Livraria.API.Middleware;

/// <summary>
/// Middleware para tratamento centralizado de exceções.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        int statusCode;
        string message;

        switch (exception)
        {
            case DomainException domainEx:
                (statusCode, message) = HandleDomainException(domainEx);
                break;

            case ValidationException validationEx:
                (statusCode, message) = HandleValidationException(validationEx);
                break;

            case DbUpdateConcurrencyException:
                (statusCode, message) = HandleConcurrencyException();
                break;

            case DbUpdateException dbEx:
                (statusCode, message) = HandleDbUpdateException(dbEx);
                break;

            default:
                (statusCode, message) = HandleUnknownException(exception);
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new { message };
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private (int StatusCode, string Message) HandleDomainException(DomainException ex)
    {
        _logger.LogWarning("Erro de domínio: {Message}", ex.Message);
        return (StatusCodes.Status400BadRequest, ex.Message);
    }

    private (int StatusCode, string Message) HandleValidationException(ValidationException ex)
    {
        var errors = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage));
        _logger.LogWarning("Erro de validação: {Errors}", errors);
        return (StatusCodes.Status400BadRequest, errors);
    }

    private (int StatusCode, string Message) HandleDbUpdateException(DbUpdateException ex)
    {
        if (ex.InnerException is SqlException sqlEx)
        {
            _logger.LogError(ex, "Erro de banco de dados: {Number} - {Message}", sqlEx.Number, sqlEx.Message);

            return sqlEx.Number switch
            {
                2627 or 2601 => (StatusCodes.Status409Conflict, "Registro duplicado. Já existe um registro com esses dados."),
                547 => (StatusCodes.Status400BadRequest, "Referência inválida. Verifique se os dados relacionados existem."),
                _ => (StatusCodes.Status500InternalServerError, "Erro ao acessar o banco de dados.")
            };
        }

        _logger.LogError(ex, "Erro de banco de dados");
        return (StatusCodes.Status500InternalServerError, "Erro ao acessar o banco de dados.");
    }

    private (int StatusCode, string Message) HandleConcurrencyException()
    {
        _logger.LogWarning("Conflito de concorrência detectado");
        return (StatusCodes.Status409Conflict, "O registro foi modificado por outro usuário. Por favor, recarregue os dados e tente novamente.");
    }

    private (int StatusCode, string Message) HandleUnknownException(Exception ex)
    {
        _logger.LogError(ex, "Erro não tratado: {Message}", ex.Message);
        return (StatusCodes.Status500InternalServerError, "Ocorreu um erro interno. Por favor, tente novamente mais tarde.");
    }
}

/// <summary>
/// Extensões para registrar o middleware de tratamento de exceções.
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}