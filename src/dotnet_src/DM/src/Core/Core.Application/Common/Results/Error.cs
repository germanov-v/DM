using Core.Domain.BoundedContext.Identity.Errors;
using Core.Domain.SharedKernel.Results;

namespace Core.Application.Common.Results;

public enum ErrorType
{
    None = 0,
    Validation,
    NotFound,
    Conflict,
    Forbidden,
    Failure,
    BadRequest,
    Unauthorized
}

public sealed record Error(string Message, ErrorType Type = ErrorType.None, int Code=ErrorCode.Unknown)
{
    public static Error None => new(string.Empty);
}


public static class DomainErrorMapping
{
    public static Error ToError(this DomainError error) =>
        error.Code switch
        {
            IdentityErrors.EmailEmptyCode => new Error(error.Message, ErrorType.Validation),
             _ => new Error(error.Message, ErrorType.Failure),
        };
}

