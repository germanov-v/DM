using Core.Constants.Database;
using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.BoundedContext.Identity.Repositories;
using Core.Domain.BoundedContext.Identity.ValueObjects;
using Core.Domain.SharedKernel.ValueObjects;
using Dapper;
using Npgsql;

namespace Core.Infrastructure.Persistence.Repositories.Identity;

public class UserRepository : IUserRepository
{
    private readonly IConnectionFactory<NpgsqlConnection> _connectionFactory;
    


    public UserRepository(IConnectionFactory<NpgsqlConnection> connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IdGuid> Create(User entity,string[] roleAliases, CancellationToken cancellationToken)
    {
        if (entity.Email==null)
             throw new ArgumentNullException($"{nameof(entity)}.{nameof(entity.Email)}");
        
        if (entity.Password==null)
            throw new ArgumentNullException($"{nameof(entity)}.{nameof(entity.Password)}");
        
        var connection = await _connectionFactory.GetCurrentConnection(cancellationToken);
        
        bool needCommitInternal = false;
        if (_connectionFactory.CurrentTransaction!=null)
        {
           await  _connectionFactory.InitialTransaction(cancellationToken);
                 needCommitInternal = true;
        }
        
        const string sql = @$"
               INSERT INTO {SchemasDbConstants.Identity}.users
                       (guid_id, name)
                VALUES(@GuidId, @Name)
                RETURNING id, guid_id
              ";

     
        var resultId = await connection.QuerySingleAsync<(long Id, Guid GuidId)>(new CommandDefinition(
            sql,
            new
            {
                GuidId = entity.Id.ValueGuid,
                Name = entity.Name.Value
            },
            cancellationToken: cancellationToken,
            transaction: _connectionFactory.CurrentTransaction
        ));

        var id = new IdGuid(resultId.Id, resultId.GuidId);
        
        const string roles = @$"
               INSERT INTO {SchemasDbConstants.Identity}.users_roles
                       (user_id, role_id)
                SELECT @UserId, r.id FROM {SchemasDbConstants.Identity}.roles r
                WHERE r.alias = ANY (@RoleAliases)
              ";

     
       var countCreatedRoles = await connection.ExecuteAsync(new CommandDefinition(
           roles,
            new
            {
                UserId = id.ValueLong,
                RoleAliases = roleAliases,
            },
            cancellationToken: cancellationToken,
            transaction: _connectionFactory.CurrentTransaction
        ));
        if (countCreatedRoles != roleAliases.Length)
        {
            await _connectionFactory.RollbackTransaction(cancellationToken);
            throw new InvalidOperationException($"One of the roles not assigned by user CountCreated: {countCreatedRoles} CountRoleAliases: {roleAliases.Length}");
        }
       
        const string emailSql = @$"
               INSERT INTO {SchemasDbConstants.Identity}.users_email
                       (user_id, email, password_hash, password_salt, is_confirmed)
                VALUES (@UserId, @Email, @PasswordHash, @PasswordSalt, @IsConfirmed)
              ";

     
        _ = await connection.ExecuteAsync(new CommandDefinition(
            emailSql,
            new
            {
                UserId = id.ValueLong,
                Email = entity.Email.Value,
                PasswordHash = entity.Password.PasswordHash,
                PasswordSalt = entity.Password.PasswordSalt,
                IsConfirmed = entity.IsActive
            },
            cancellationToken: cancellationToken,
            transaction: _connectionFactory.CurrentTransaction
        ));
        
        
        
        if (needCommitInternal)
        {
           await  _connectionFactory.CommitTransaction(cancellationToken);
        }

        return id;
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

    
    record struct UserEmailNonPasswordSqlIdentity(long Id, 
        Guid GuidId,
        bool IsActive,
        string Email,
        string Name,
        long RoleId, 
        string RoleAlias,
        string RoleName,
        Guid RoleGuid);
    public async Task<User?> GetByEmail(string email, CancellationToken cancellationToken, bool isConfirmed = true)
    {
        const string sql = @$"
              SELECT 
                     us.id AS id,
                     us.guid_id AS guid_id,
                     us.is_active AS is_active,
                     ue.email AS email,
                     r.id AS role_id,
                     r.alias AS role_alias,
                     r.name AS role_name,
                     r.guid_id AS role_guid_id
                            FROM {SchemasDbConstants.Identity}.users AS us
                            INNER JOIN {SchemasDbConstants.Identity}.users_email AS ue ON us.id = ue.user_id
                            INNER JOIN {SchemasDbConstants.Identity}.users_roles AS ur ON us.id = ur.user_id
                            INNER JOIN {SchemasDbConstants.Identity}.roles AS r ON r.id = ur.role_id
              WHERE ue.email=@Email
            ";

        var connection = await _connectionFactory.GetCurrentConnection(cancellationToken);
        var result = await this.QueryMultiByMapperFirst<UserEmailNonPasswordSqlIdentity, User, long>(sql,
            connection,
            cancellationToken,
             (in UserEmailNonPasswordSqlIdentity r, ref Dictionary<long, User> dict) =>
            {
                if (!dict.TryGetValue(r.Id, out var user))
                {
                  user = new User(new EmailIdentity(r.Email), r.IsActive,
                      new Name(r.Name),
                      null,
                      new IdGuid(r.Id, r.GuidId));
                    dict.Add(r.Id,user);
                }
                user.AddRole(new Role(new IdGuid(r.RoleId, r.RoleGuid), r.RoleName, r.RoleAlias));
            },
            new
            {
                Email = email
            }
        );
        
        return result;
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