using Core.Domain.SharedKernel.Results;

namespace Core.Domain.BoundedContext.Identity.Errors;

public static class IdentityErrors
{
    // public static DomainError EmailNotUnique(string value)
    //      => new($"Email '{value}' is already used.", );

    public const string EmailEmptyCode = "email_empty";
    
    public static DomainError EmailEmpty()
        => new DomainError(EmailEmptyCode, "Email is empty");
}