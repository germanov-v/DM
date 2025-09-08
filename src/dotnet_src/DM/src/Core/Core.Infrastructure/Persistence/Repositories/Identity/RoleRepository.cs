using Core.Application.Abstractions;
using Core.Constants.Database;
using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.BoundedContext.Identity.Repositories;
using Core.Domain.SharedKernel.ValueObjects;
using Dapper;
using Npgsql;

using SqlRoleRow = (long Id, System.Guid GuidId, string Name, string Alias);


namespace Core.Infrastructure.Persistence.Repositories.Identity;

public class RoleRepository : IRoleRepository
{
    private readonly IConnectionFactory<NpgsqlConnection> _connectionFactory;

    public RoleRepository(IConnectionFactory<NpgsqlConnection> connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    private const string fieldsRole = """
                                      role.id, role.guid_id,role.name,role.alias
                                      """;

    public async Task<IdGuid> Create(Guid guidId, string name, string alias, CancellationToken cancellationToken)
    {
        const string sql = $"""
                             INSERT INTO {SchemasDbConstants.Identity}.roles
                                     (guid_id, name, alias)
                              VALUES(@GuidId, @Name, @Alias)
                              RETURNING id , guid_id
                            """;

        var connection = await _connectionFactory.GetCurrentConnection(cancellationToken);

        var resultSql = await connection.QuerySingleAsync<(long, Guid)>(new CommandDefinition(
            sql,
            new
            {
                GuidId = guidId,
                Name = name,
                Alias = alias
            },
            cancellationToken: cancellationToken,
            transaction: _connectionFactory.CurrentTransaction
        ));


        return new IdGuid(resultSql.Item1, resultSql.Item2);
    }

   // record  SqlRoleRow(long Id, Guid GuidId, string Name, string Alias);

    public async Task<Role?> GetByAlias(string alias, CancellationToken cancellationToken)
    {
        const string sql = @$"
              SELECT 
                  {fieldsRole}
              
              FROM {SchemasDbConstants.Identity}.roles AS role
              WHERE role.alias=@Alias
            ";

        var connection = await _connectionFactory.GetCurrentConnection(cancellationToken);
        var resultSql = await connection.QueryFirstOrDefaultAsync<SqlRoleRow>(
            new CommandDefinition(sql,
                new
                {
                    Alias = alias
                },
                cancellationToken: cancellationToken
                
            )
        );
        if (resultSql==default)
        {
            return null;
        }
        return new Role(new IdGuid(resultSql.Id, resultSql.GuidId), resultSql.Name, resultSql.Alias);

        // var result = await this.QuerySingleByMapper<SqlRoleRow, Role>(sql,
        //     connection,
        //     cancellationToken,
        //     static (in SqlRoleRow r) =>
        //     {
        //         var role = new Role(new IdGuid(r.Id, r.GuidId), r.Name, r.Alias);
        //         return role;
        //     },
        //     new
        //     {
        //         Alias = alias
        //     }
        // );
        // return result;
     
        //
        //
        //
        // var role = new Role(new IdGuid(resultSql.Id, resultSql.GuidId), resultSql.Name, resultSql.Alias);
    }

    public async Task<IReadOnlyList<Role>> GetListByAliases(string[] aliases, CancellationToken cancellationToken)
    {
        const string sql = $"""
              SELECT {fieldsRole} FROM {SchemasDbConstants.Identity}.roles AS role
              WHERE role.alias = ANY(@Aliases)
            """;

        var connection = await _connectionFactory.GetCurrentConnection(cancellationToken);


        // var result = await this.QueryMultiByMapper<SqlRoleRow, Role, long>(sql,
        //     connection,
        //     cancellationToken,
        //     static (in SqlRoleRow r, ref Dictionary<long, Role> dict) =>
        //     {
        //         var role = new Role(new IdGuid(r.Id, r.GuidId), r.Name, r.Alias);
        //         dict.Add(r.Id, role);
        //     },
        //     new
        //     {
        //         Aliases = aliases
        //     }
        // );
        //
        // return result;

        var sqlResult = await connection.QueryAsync < SqlRoleRow > (new CommandDefinition(sql, new
        {
            Aliases = aliases
        },
            cancellationToken: cancellationToken));
        List<Role> result = new List<Role>();
        foreach (var item in sqlResult)
        {
            result.Add(new Role(new IdGuid(item.Id, item.GuidId), item.Name, item.Alias));
        }
        return result;
    }

    public async Task<int> UpdateRoles(IdGuid id,
        string[] roleAliases,
        CancellationToken cancellationToken)
    {
        var connection = await _connectionFactory.GetCurrentConnection(cancellationToken);

        bool existExternalTransaction = true;
        if (_connectionFactory.CurrentTransaction is null)
        {
            await _connectionFactory.InitialTransaction(cancellationToken);
            existExternalTransaction = false;
        }

        const string removeRoles = $"""

                                    DELETE FROM {SchemasDbConstants.Identity}.users_roles
                                           WHERE
                                               user_id=@Id;

                                    """;


        _ = await connection.ExecuteAsync(new CommandDefinition(removeRoles,
            new
            {
                Id = id.ValueLong,
            },
            transaction: _connectionFactory.CurrentTransaction,
            cancellationToken: cancellationToken)
        );


        const string insertRoles = $"""
                                    INSERT INTO {SchemasDbConstants.Identity}.users_roles
                                                        (role_id, user_id)
                                                 SELECT 
                                                    r.id, @Id
                                     FROM 
                                          {SchemasDbConstants.Identity}.roles r 
                                    WHERE r.alias = ANY(@Roles) 
                                    """;


        var result = await connection.ExecuteAsync(new CommandDefinition(insertRoles,
            new
            {
                Id = id.ValueLong,
                Roles = roleAliases,
            },
            transaction: _connectionFactory.CurrentTransaction,
            cancellationToken: cancellationToken)
        );


        if (!existExternalTransaction)
        {
            await _connectionFactory.CommitTransaction(cancellationToken);
        }

        return result;
    }
}