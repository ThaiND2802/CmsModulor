using CommerceCore.SharedKernel.Exceptions;
using FluentValidation;
using MediatR;

namespace CommerceCore.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators ?? throw new ArgumentNullException(nameof(validators));

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(_validators.Select(x => x.ValidateAsync(context, cancellationToken)));
        var failures = validationResults
            .SelectMany(static x => x.Errors)
            .Where(static x => x is not null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        var errors = failures
            .GroupBy(
                static x => string.IsNullOrWhiteSpace(x.PropertyName) ? string.Empty : x.PropertyName,
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                static group => group.Key,
                static group => (IReadOnlyList<string>)group
                    .Select(static x => x.ErrorMessage)
                    .Where(static x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.Ordinal)
                    .ToList(),
                StringComparer.OrdinalIgnoreCase);

        var firstErrorMessage = errors
            .SelectMany(static x => x.Value)
            .FirstOrDefault(static x => !string.IsNullOrWhiteSpace(x));

        throw new ValidationAppException(firstErrorMessage ?? "The request payload is invalid.", errors);
    }
}
