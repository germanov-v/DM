using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.BoundedContext.Identity.Repositories;

namespace Core.Infrastructure.Persistence.Repositories.Identity;

public class UserRepository : IUserRepository
{
    public Task<User> Create(User entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<int> Update(User entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<int> UpdateRoles(string email, string[] roleAliases, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetById(long id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetByGuidId(Guid guidId, CancellationToken cancellationToken, bool isConfirmed = true)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetByEmail(string email, CancellationToken cancellationToken, bool isConfirmed = true)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetByEmailWithProfiles(string email, CancellationToken cancellationToken, bool isConfirmed = true)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetByEmailAndHashPassword(string email, string passwordHash, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}