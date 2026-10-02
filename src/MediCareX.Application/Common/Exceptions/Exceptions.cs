using FluentValidation.Results;

namespace MediCareX.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string entity, object key) : base($"{entity} ({key}) was not found.") { }
}

public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string message = "You do not have permission to perform this action.") : base(message) { }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Authentication failed.") : base(message) { }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

/// <summary>Domain/business rule violation (maps to HTTP 422).</summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException() : base("One or more validation failures have occurred.")
        => Errors = new Dictionary<string, string[]>();

    public ValidationException(IEnumerable<ValidationFailure> failures) : this()
        => Errors = failures.GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                            .ToDictionary(g => g.Key, g => g.ToArray());

    public ValidationException(string property, string message) : this()
        => Errors = new Dictionary<string, string[]> { [property] = new[] { message } };

    public ValidationException(string property, IEnumerable<string> messages) : this()
        => Errors = new Dictionary<string, string[]> { [property] = messages.ToArray() };
}
