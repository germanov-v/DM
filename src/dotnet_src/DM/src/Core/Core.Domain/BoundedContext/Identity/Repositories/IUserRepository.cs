using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.SharedKernel.Abstractions;

namespace Core.Domain.BoundedContext.Identity.Repositories;

public interface IUserRepository : IRepository
{
    Task<User> Create(User entity, CancellationToken cancellationToken);

    Task<int> Update(User entity, CancellationToken cancellationToken);

    Task<int> UpdateRoles(string email, string[] roleAliases,
        CancellationToken cancellationToken);

    Task<User?> GetById(long id, CancellationToken cancellationToken);

    Task<User?> GetByGuidId(Guid guidId, CancellationToken cancellationToken, bool isConfirmed = true);
    
    Task<User?> GetByEmail(string email, CancellationToken cancellationToken, bool isConfirmed= true);

    Task<User?> GetByEmailWithProfiles(string email, CancellationToken cancellationToken, bool isConfirmed = true);
    
    Task<User?> GetByEmailAndHashPassword(string email, string passwordHash, CancellationToken cancellationToken);
}