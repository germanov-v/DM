using Core.Application.Abstractions.Services;
using Core.Application.Common.Results;
using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.SharedKernel.Results;
using Core.Domain.SharedKernel.ValueObjects;

namespace Core.Application.Abstractions.BusinessLogic.Identity;

public interface IEmailPasswordUserProvider : IApplicationService
{
    Task<Result<IdGuid>> Create(string email, string password, string name, string[] roleAliases, 
        CancellationToken cancellationToken
        , bool isActive = false);

    Task<Result<User>> GetUserByCredentialsAndRole(string email, string password, string roleAlias,
        CancellationToken cancellationToken);
    

    Task<DomainResult<User>> GetUserByCredential(string email, CancellationToken cancellationToken);

    Task<bool> IsInRole(User user, string role, CancellationToken cancellationToken);
}