using System.Net;
using Core.Constants.Database;
using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.BoundedContext.Identity.Repositories;
using Core.Domain.BoundedContext.Identity.ValueObjects;
using Core.Domain.SharedKernel.ValueObjects;
using Core.Infrastructure.Persistence.Mappers;
using Dapper;
using Npgsql;

namespace Core.Infrastructure.Persistence.Repositories.Identity;

public class SessionRepository : ISessionRepository
{
    #region Fields Sql

    // language=SQL
    const string QueryUserFields = """
                                 
                            
                                  """;
    // language=SQL
    private const string QueryUserFrom = $"""
                                               
                                               """;


    private record QuerySessionBaseResult(
        long Id,
        long UserId,
        string Provider,
        Guid GuidId,
        DateTimeOffset CreatedAt,
        string AccessToken,
        string RefreshToken,
        DateTimeOffset RefreshTokenExpiresAt,
        string Fingerprint,
        IPAddress Ip) : ISqlFields;

    
    
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
                        refresh_token_expires_at,
                        fingerprint,
                        ip, 
                        provider)
                VALUES(@UserId, 
                       @GuidId, 
                       @CreatedAt,
                       @AccessToken,
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
                CreatedAt = entity.CreateAt.Value,
                AccessToken = entity.AccessToken,
                RefreshToken = entity.RefreshToken,
                RefreshExpired = entity.RefreshTokenExpiresAt.Value,
                Fingerprint = entity.Fingerprint??String.Empty,
                Ip = entity.Ip,
                Provider = entity.AuthProvider.Alias
            },
            cancellationToken: cancellationToken,
            transaction: _connectionFactory.CurrentTransaction
        ));
        
        return new IdGuid(resultId.Id, resultId.GuidId);
    }

    public async Task<int> RemoveById(long id, CancellationToken cancellationToken)
    {
        const string sql = $"""
                           DELETE FROM {SchemasDbConstants.Identity}.sessions
                           WHERE id = @Id
                           """;
        var connection = await _connectionFactory.GetCurrentConnection(cancellationToken);
        
        
        
        return await connection.ExecuteAsync(sql, new { Id = id }, transaction: _connectionFactory.CurrentTransaction);
    }

    public async Task<Session?> GetByRefreshTokenFingerprint(string refreshToken, string? fingerPrint, DateTimeOffset actualDate,
        CancellationToken cancellationToken)
    {
        
        const string sql = $"""
                           SELECT 
                               id,    
                               user_id, 
                               guid_id,
                               created_at,
                               access_token,
                               refresh_token,
                               refresh_token_expires_at,
                               fingerprint,
                               ip, 
                               provider
                           FROM {SchemasDbConstants.Identity}.sessions 
                           WHERE refresh_token=@RefreshToken
                             AND fingerprint=@FingerPrint
                           """;
        
        var connection = await _connectionFactory.GetCurrentConnection(cancellationToken);

        var resultSql = await connection.QueryFirstOrDefaultAsync<QuerySessionBaseResult>(
            new CommandDefinition(sql,
                new
                {
                    RefreshToken = refreshToken,
                    FingerPrint = fingerPrint??string.Empty,
                },
                cancellationToken: cancellationToken,
                transaction: _connectionFactory.CurrentTransaction)
        );
        
        Session? session = null;

        if (resultSql != null)
        {
            session = new Session(resultSql.AccessToken, 
                resultSql.RefreshToken, 
                resultSql.UserId, 
                createdAt: new AppDate(resultSql.CreatedAt),
                refreshTokenExpiresAt:  new AppDate(resultSql.RefreshTokenExpiresAt),
                ip: resultSql.Ip,
                fingerprint: resultSql.Fingerprint,
                authProvider:  AuthProvider.GetValue(resultSql.Provider),
                id: new IdGuid(resultSql.Id, resultSql.GuidId));
        }
        
        
        return session;
    }
}