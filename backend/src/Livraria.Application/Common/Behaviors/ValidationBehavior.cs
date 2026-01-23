using FluentValidation;
using Livraria.Application.Common.Models;
using MediatR;

namespace Livraria.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior que executa validações FluentValidation antes do handler.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            var errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));

            // Se o tipo de retorno é Result<T>, retorna um Failure
            if (typeof(TResponse).IsGenericType &&
                typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var resultType = typeof(TResponse);
                var failureMethod = resultType.GetMethod("Failure");
                var error = Error.Validation(errorMessage);
                return (TResponse)failureMethod!.Invoke(null, [error])!;
            }

            // Se o tipo de retorno é Result (sem generic), retorna um Failure
            if (typeof(TResponse) == typeof(Result))
            {
                return (TResponse)(object)Result.Failure(Error.Validation(errorMessage));
            }

            throw new ValidationException(failures);
        }

        return await next();
    }
}