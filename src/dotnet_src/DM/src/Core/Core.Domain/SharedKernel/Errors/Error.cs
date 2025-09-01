namespace Core.Domain.SharedKernel.Errors;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Forbidden,
    Failure,
    None,
}

public sealed record Error(string Message, ErrorType Type = ErrorType.None)
{
    public static Error None => new(string.Empty, ErrorType.None);
}