using Core.Domain.SharedKernel.Results;

namespace Core.Domain.BoundedContext.Identity.Errors;

public static class IdentityErrors
{
    public static Error EmailNotUnique(string value)
         => new("EmailNotUnique", $"Email '{value}' is already used.", ErrorType.Conflict);
}