namespace Core.Domain.SharedKernel.Results;

public readonly record struct DomainError(string Code, string Message, params object[] Errors);

