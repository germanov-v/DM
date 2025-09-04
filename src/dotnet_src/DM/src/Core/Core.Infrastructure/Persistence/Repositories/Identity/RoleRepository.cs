using Core.Application.Abstractions;
using Core.Constants.Database;
using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.BoundedContext.Identity.Repositories;
using Core.Domain.SharedKernel.ValueObjects;
using Dapper;
using Npgsql;
//using SqlRoleRow = (long Id, System.Guid GuidId, string Name, string Alias);



namespace Core.Infrastructure.Persistence.Repositories.Identity;



public class RoleRepository : IRoleRepository
{
    private readonly IConnectionFactory<NpgsqlConnection> _connectionFactory;

    public RoleRepository(IConnectionFactory<NpgsqlConnection> connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }



    public async Task<IdGuid> Create(Guid guidId, string name, string alias, CancellationToken cancellationToken)
    {
        const string sql = @$"
               INSERT INTO {SchemasDbConstants.Identity}.roles
                       (guid_id, title, alias)
                VALUES(@GuidId, @Title, @Alias)
                RETURNING id, guid_id
              ";

        var connection = await _connectionFactory.GetCurrentConnection(cancellationToken);

        var id = await connection.QuerySingleAsync<IdGuid>(new CommandDefinition(
            sql,
            new
            {
                GuidId = guidId,
                Title = name,
                Alias = alias
            },
            cancellationToken: cancellationToken,
            transaction: _connectionFactory.CurrentTransaction
        ));


       

        return id;
    }
    
    readonly record struct SqlRoleRow(long Id, Guid GuidId, string Name, string Alias);

    public async Task<Role?> GetByAlias(string alias, CancellationToken cancellationToken)
    {
        const string sql = @$"
              SELECT role.* FROM {SchemasDbConstants.Identity}.roles AS role
              WHERE role.alias=@Alias
            ";

        var connection = await _connectionFactory.GetCurrentConnection(cancellationToken);
        // var result = await connection.QuerySingleOrDefaultAsync<Role>(sql, new
        // {
        //     Alias = alias
        // });



        // static Role Map(in (long Id, 
        //     Guid guidId, 
        //     string Name,
        //     string Alias
        //     ) r)
        // {
        //     var role = new Role(new IdGuid(r.Id, r.guidId), r.Name, r.Alias);
        //     return role;
        // }
       
        var result = await this.QuerySingleByMapper<SqlRoleRow, Role>(sql,
            connection,
            cancellationToken,
             static (in SqlRoleRow r) =>
            {
                var role = new Role(new IdGuid(r.Id, r.GuidId), r.Name, r.Alias);
                return role;
            },
            new
            {
                Alias = alias
            }
        );
        
        return result;
    }

    public Task<List<Role>> GetListByAliases(string[] aliases, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
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

                                    DELETE FROM {SchemasDbConstants.Identity}.user_roles
                                           WHERE
                                               user_id=@Id;

                                    """;


        _ = await connection.ExecuteAsync(new CommandDefinition(removeRoles,
            new
            {
                Id = id.ToString(),
            },
            transaction: _connectionFactory.CurrentTransaction,
            cancellationToken: cancellationToken)
        );


        const string insertRoles = $"""
                                     INSERT INTO {SchemasDbConstants.Identity}.user_roles
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