using Core.Constants.Database;
using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.BoundedContext.Identity.Repositories;
using Core.Domain.SharedKernel.ValueObjects;
using Dapper;
using Npgsql;

namespace Core.Infrastructure.Persistence.Repositories.Identity;

public class SessionRepository : ISessionRepository
{
    #region Fields Sql

    // language=SQL
    const string QueryUserFields = """
                                  users.id AS id,
                                  users.guid_id AS guid_id,
                                  users.name AS name,
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
        bool IsActive,
        string Email,
        long RoleId,
        string RoleAlias,
        string RoleName,
        Guid RoleGuidId) : ISqlFields;

    
    
    #endregion
    
    private readonly IConnectionFactory<NpgsqlConnection> _connectionFactory;

    public SessionRepository(IConnectionFactory<NpgsqlConnection> connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IdGuid> Create(Session entity, CancellationToken cancellationToken)
    {
        const string sql = $"""
               INSERT INTO {SchemasDbConstants.Identity}.sessions
                       (user_id, 
                        guid_id,
                        created_at,
                        access_token,
                        refresh_token,
                        refresh_expired,
                        fingerprint,
                        ip, 
                        provider)
                VALUES(@UserId, 
                       @GuidId, 
                       @CreatedAt,
                       @RefreshToken,
                       @RefreshExpired,
                       @Fingerprint,
                       @Ip,
                       @Provider
                       )
                RETURNING id, guid_id
              """;

        var connection = await _connectionFactory.GetCurrentConnection(cancellationToken);

        var resultId = await connection.QuerySingleAsync<(long Id, Guid GuidId)>(new CommandDefinition(
            sql,
            new
            {
                UserId = entity.UserId,
                GuidId = IdGuid.New().ValueGuid,
                CreatedAt = entity.CreateAt,
                AccessToken = entity.AccessToken,
                RefreshToken = entity.RefreshToken,
                RefreshExpired = entity.RefreshExpired,
                Fingerprint = entity.Fingerprint,
                Ip = entity.Ip,
                Provider = entity.AuthProvider.Name
            },
            cancellationToken: cancellationToken,
            transaction: _connectionFactory.CurrentTransaction
        ));
        
        return new IdGuid(resultId.Id, resultId.GuidId);
    }

    public Task<int> Remove(Session entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Session?> GetByRefreshTokenFingerprint(string refreshToken, string fingerPrint, DateTimeOffset actualDate,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}