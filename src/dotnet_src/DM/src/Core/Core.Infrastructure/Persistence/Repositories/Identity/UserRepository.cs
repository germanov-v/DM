using System.Runtime.InteropServices;
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
    #region Fields Sql

    // language=SQL
    const string QueryUserFields = """
                                   users.id AS id,
                                   users.guid_id AS guid_id,
                                   users.name AS name,
                                   users.created_at AS created_at,
                                   users.is_active AS is_active,
                                   users_email.email AS email,
                                   roles.id AS role_id,
                                   roles.alias AS role_alias,
                                   roles.name AS role_name,
                                   roles.guid_id AS role_guid_id
                                   """;

    // language=SQL
    private const string QueryUserFrom = $"""
                                          FROM {SchemasDbConstants.Identity}.users AS users
                                          INNER JOIN {SchemasDbConstants.Identity}.users_email AS users_email ON users.id = users_email.user_id
                                          INNER JOIN {SchemasDbConstants.Identity}.users_roles AS users_roles ON users.id = users_roles.user_id
                                          INNER JOIN {SchemasDbConstants.Identity}.roles AS roles ON roles.id = users_roles.role_id

                                          """;


    private record QueryUserBaseResult(
        long Id,
        string Name,
        Guid GuidId,
        DateTimeOffset CreatedAt,
        bool IsActive,
        string Email,
        long RoleId,
        string RoleAlias,
        string RoleName,
        Guid RoleGuidId) : ISqlFields;

    private record QueryUserEmailCredentialsResult(
        long Id,
        string Name,
        Guid GuidId,
        DateTimeOffset CreatedAt,
        bool IsActive,
        string Email,
        long RoleId,
        string RoleAlias,
        string RoleName,
        Guid RoleGuidId,
        string PasswordHash,
        string PasswordSalt,
        bool IsConfirmed,
        DateTimeOffset ConfirmedChangedAt,
        string ConfirmedCode,
        string ConfirmedCodeExpiresAt
    ) : QueryUserBaseResult(Id, Name, GuidId, CreatedAt, IsActive, Email, RoleId, RoleAlias, RoleName, RoleGuidId);

    #endregion


    private readonly IConnectionFactory<NpgsqlConnection> _connectionFactory;


    public UserRepository(IConnectionFactory<NpgsqlConnection> connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IdGuid> Create(User entity, string[] roleAliases, CancellationToken cancellationToken)
    {
        if (entity.Email == null)
            throw new ArgumentNullException($"{nameof(entity)}.{nameof(entity.Email)}");

        if (entity.Password == null)
            throw new ArgumentNullException($"{nameof(entity)}.{nameof(entity.Password)}");

        var connection = await _connectionFactory.GetCurrentConnection(cancellationToken);

        bool needCommitInternal = false;
        if (_connectionFactory.CurrentTransaction != null)
        {
            await _connectionFactory.InitialTransaction(cancellationToken);
            needCommitInternal = true;
        }

        const string sql = @$"
               INSERT INTO {SchemasDbConstants.Identity}.users
                       (guid_id, name, is_active)
                VALUES(@GuidId, @Name, @IsActive)
                RETURNING id, guid_id
              ";


        var resultId = await connection.QuerySingleAsync<(long Id, Guid GuidId)>(new CommandDefinition(
            sql,
            new
            {
                GuidId = entity.Id.ValueGuid,
                Name = entity.Name.Value,
                IsActive = entity.IsActive,
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
            throw new InvalidOperationException(
                $"One of the roles not assigned by user CountCreated: {countCreatedRoles} CountRoleAliases: {roleAliases.Length}");
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
            await _connectionFactory.CommitTransaction(cancellationToken);
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


    // TODO: порядок кортежа
    public async Task<User?> GetByEmailV1(string email, CancellationToken cancellationToken, bool isConfirmed = true)
    {
        const string sql = $"""
                                       SELECT 
                                        {QueryUserFields}
                                        {QueryUserFrom}  
                            WHERE users_email.email=@Email
                            """;

        var connection = await _connectionFactory.GetCurrentConnection(cancellationToken);

        var resultSql = await connection.QueryAsync<QueryUserBaseResult>(
            new CommandDefinition(sql,
                new
                {
                    Email = email,
                },
                cancellationToken: cancellationToken,
                transaction: _connectionFactory.CurrentTransaction)
        );
        Dictionary<long, User> dict = new(1);


        foreach (var item in resultSql)
        {
            ref User? userRef = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, item.Id, out bool exists);
            //   ref User? userRef = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, item.Id, out bool exists);

            if (!exists)
            {
                userRef = new User(new EmailIdentity(item.Email), item.IsActive,
                    new Name(item.Name),
                    null,
                    new IdGuid(item.Id, item.GuidId));
            }

            if (userRef == null)
                throw new InvalidOperationException($"User with email {item.Email} not mapped");
            userRef.AddRole(new Role(new IdGuid(item.RoleId, item.RoleGuidId), item.RoleName, item.RoleAlias));
        }

        return dict.Values.FirstOrDefault();
    }

    public async Task<User?> GetByEmail(string email, CancellationToken cancellationToken, bool isConfirmed = true)
    {
        const string sql = $"""
                                       SELECT 
                                        {QueryUserFields}
                                        {QueryUserFrom}   
                                        WHERE users_email.email=@Email
                            """;

        var connection = await _connectionFactory.GetCurrentConnection(cancellationToken);

        // var resultSql = (await connection.QueryAsync<
        //     (long Id,
        //     Guid GuidId, string Name, bool IsActive, string Email,
        //     long RoleId, string RoleAlias, string RoleName, Guid RoleGuid)>(
        //     new CommandDefinition(sql,
        //         new
        //         {
        //             Email = email,
        //         },
        //         cancellationToken: cancellationToken,
        //         transaction: _connectionFactory.CurrentTransaction)
        // )).AsList();


        var resultSql = (await connection.QueryAsync<QueryUserBaseResult>(
            new CommandDefinition(sql,
                new
                {
                    Email = email,
                },
                cancellationToken: cancellationToken,
                transaction: _connectionFactory.CurrentTransaction)
        )).AsList();
        Dictionary<long, User> dict = new(1);

        var span = CollectionsMarshal.AsSpan(resultSql);

        foreach (ref readonly var item in span)
        {
            ref User? userRef = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, item.Id, out bool exists);
            //   ref User? userRef = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, item.Id, out bool exists);

            if (!exists)
            {
                userRef = new User(new EmailIdentity(item.Email), item.IsActive,
                    new Name(item.Name),
                    null,
                    new IdGuid(item.Id, item.GuidId),
                    createdAt: new AppDate(item.CreatedAt));
            }

            if (userRef == null)
                throw new InvalidOperationException($"User with email {item.Email} not mapped");
            userRef.AddRole(new Role(new IdGuid(item.RoleId, item.RoleGuidId), item.RoleName, item.RoleAlias));
        }

        return dict.Values.FirstOrDefault();
    }

    public Task<User?> GetByEmailWithProfiles(string email, CancellationToken cancellationToken,
        bool isConfirmed = true)
    {
        throw new NotImplementedException();
    }

    public async Task<User?> GetEmailCredentialsUserByEmail(string email, CancellationToken cancellationToken)
    {
        // language=sql
        const string sql = $"""
                                       SELECT 
                                        {QueryUserFields},
                                        users_email.password_hash as password_hash,
                                        users_email.password_salt as password_salt,
                                        users_email.is_confirmed as is_confirmed,
                                        users_email.confirmed_changed_at as confirmed_changed_at,
                                        users_email.confirmed_code as confirmed_code,
                                        users_email.confirmed_code_expires_at as confirmed_code_expires_at
                                        {QueryUserFrom}   
                                        WHERE users_email.email=@Email
                            """;

        var connection = await _connectionFactory.GetCurrentConnection(cancellationToken);

        var resultSql = (await connection.QueryAsync<QueryUserEmailCredentialsResult>(
            new CommandDefinition(sql,
                new
                {
                    Email = email,
                },
                cancellationToken: cancellationToken,
                transaction: _connectionFactory.CurrentTransaction)
        )).AsList();


        User? user = null;
        foreach (var item in resultSql)
        {
            if (user == null)
            {
                user = new User(new EmailIdentity(item.Email),
                    item.IsActive,
                    new Name(item.Name),
                    id: new IdGuid(item.Id, item.GuidId),
                    password: new Password(item.PasswordHash, item.PasswordSalt),
                    createdAt: new AppDate(item.CreatedAt)
                );
            }

            user.AddRole(new Role(new IdGuid(item.RoleId, item.RoleGuidId), item.RoleName, item.RoleAlias));
        }

        return user;
    }
}