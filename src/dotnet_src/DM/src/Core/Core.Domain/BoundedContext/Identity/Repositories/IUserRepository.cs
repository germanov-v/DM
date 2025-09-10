using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.SharedKernel.Abstractions;
using Core.Domain.SharedKernel.ValueObjects;

namespace Core.Domain.BoundedContext.Identity.Repositories;

public interface IUserRepository : IRepository
{
    Task<IdGuid> Create(User entity, string[] roleAliases,CancellationToken cancellationToken);

    Task<int> Update(User entity, CancellationToken cancellationToken);

    Task<int> UpdateRoles(string email, string[] roleAliases,
        CancellationToken cancellationToken);

    Task<User?> GetById(long id, CancellationToken cancellationToken);

    Task<User?> GetByGuidId(Guid guidId, CancellationToken cancellationToken, bool isConfirmed = true);
    
    Task<User?> GetByEmail(string email, CancellationToken cancellationToken, bool isConfirmed= true);

    Task<User?> GetByEmailWithProfiles(string email, CancellationToken cancellationToken, bool isConfirmed = true);
    
    /// <summary>
    /// Проверяем пользователя
    /// </summary>
    /// <param name="email"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<User?> GetEmailCredentialsUserByEmail(string email, CancellationToken cancellationToken);
}