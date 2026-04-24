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

        var firstErrorMessage = failures
            .Select(static x => x.ErrorMessage)
            .FirstOrDefault(static x => !string.IsNullOrWhiteSpace(x));

        throw new ValidationAppException(firstErrorMessage ?? "The request payload is invalid.");
    }
}
