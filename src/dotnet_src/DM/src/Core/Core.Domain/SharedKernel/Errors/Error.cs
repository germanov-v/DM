namespace Core.Domain.SharedKernel.Errors;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Forbidden,
    Failure
}

public sealed record Error(string Code, string Message, ErrorType Type = ErrorType.Failure);