namespace Core.Infrastructure.Persistence.Repositories.Identity;

internal readonly record struct UserEmailNonPasswordSqlIdentityV1(long Id, 
    Guid GuidId,
    bool IsActive,
    string Email,
    string Name,
    long RoleId, 
    string RoleAlias,
    string RoleName,
    Guid RoleGuid);