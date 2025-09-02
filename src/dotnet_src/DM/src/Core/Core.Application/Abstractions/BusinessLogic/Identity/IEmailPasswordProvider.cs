using Core.Application.Abstractions.Services;
using Core.Application.Common.Results;
using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.SharedKernel.Abstractions;
using Core.Domain.SharedKernel.Results;

namespace Core.Application.Abstractions.BusinessLogic.Identity;

public interface IEmailPasswordProvider : IApplicationService
{
    Task<Result<User>> GetUserByCredential(string email, string password, CancellationToken cancellationToken);
    Task<DomainResult<User>> GetUserByCredential(string email, CancellationToken cancellationToken);

    Task<bool> IsInRole(User user, string role, CancellationToken cancellationToken);
}