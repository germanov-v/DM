using Core.Application.Abstractions.BusinessLogic.Identity;
using Core.Application.Abstractions.Services.Identity;
using Core.Application.Common.Results;
using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.SharedKernel.Results;

namespace Core.Application.BusinessLogic.Identity;

public class EmailPasswordProvider : IEmailPasswordProvider
{
    public async Task<Result<User>> GetUserByCredential(string email, string password, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<DomainResult<User>> GetUserByCredential(string email, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> IsInRole(User user, string role, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}