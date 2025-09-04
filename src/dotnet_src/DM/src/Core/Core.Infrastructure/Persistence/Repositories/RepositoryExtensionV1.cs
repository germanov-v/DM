using System.Collections.ObjectModel;
using System.Data.Common;
using System.Runtime.CompilerServices;
using Core.Domain.SharedKernel.Abstractions;
using Dapper;

namespace Core.Infrastructure.Persistence.Repositories;

public static class RepositoryExtensionV1
{
   //  public static async Task<Dictionary<long, TResult>.ValueCollection> QueryMapperV1< TSqlResult, TResult>(
   //      this IRepository repository,
   //      string sql,
   //      DbConnection connection,
   //      CancellationToken cancellationToken,
   //      Action<TSqlResult,Dictionary<long, TResult>> mapper,
   //      object? parameters = null,
   //      DbTransaction? transaction = null)
   // where TSqlResult : struct
   //
   //  {
   //      var rows = await connection.QueryAsync<TSqlResult>(
   //          new CommandDefinition(   sql,
   //              parameters,
   //              transaction: transaction ,
   //              cancellationToken: cancellationToken )
   //    
   //      );
   //      
   //      var lookup = new Dictionary<long, TResult>();
   //
   //      foreach (var row in rows)
   //      {
   //          mapper(row, lookup);
   //      }
   //      
   //      return lookup.Values;
   //  }
    
    public static async IAsyncEnumerable<TResult> QueryMapperEnumerable<TSqlResult, TResult>(
        this IRepository repository,
        string sql,
        DbConnection connection,
        [EnumeratorCancellation] CancellationToken cancellationToken,
        Action<TSqlResult,Dictionary<long, TResult>> mapper,
        object? parameters = null,
        DbTransaction? transaction = null)
        where TSqlResult : struct

    {
        var rows = await connection.QueryAsync<TSqlResult>(
            new CommandDefinition(   sql,
                parameters,
                transaction: transaction ,
                cancellationToken: cancellationToken )
      
        );
        
        var lookup = new Dictionary<long, TResult>();

        foreach (var row in rows)
        {
             mapper(row, lookup);
        }

        foreach (var item in lookup.Values)
        {
            yield return item;
        }
    }
    
    public static async Task<IReadOnlyList<TResult>> QueryMapperListV1<TSqlResult, TResult>(
        this IRepository repository,
        string sql,
        DbConnection connection,
        CancellationToken cancellationToken,
        Action<TSqlResult,Dictionary<long, TResult>> mapper,
        object? parameters = null,
        DbTransaction? transaction = null)
        where TSqlResult : struct

    {
        var rows = await connection.QueryAsync<TSqlResult>(
            new CommandDefinition(   sql,
                parameters,
                transaction: transaction ,
                cancellationToken: cancellationToken )
      
        );
        
        var lookup = new Dictionary<long, TResult>();

        foreach (var row in rows)
        {
            mapper(row, lookup);
        }

        return lookup.Values.ToList();
    }

    public static async Task<TResult?> QueryMapperFirstV1<TSqlResult, TResult>(
        this IRepository repository,
        string sql,
        DbConnection connection,
        CancellationToken cancellationToken,
        Action<TSqlResult, Dictionary<long, TResult>> mapper,
        object? parameters = null,
        DbTransaction? transaction = null)
       where TSqlResult : struct

        => (await QueryMapperList(repository, sql, connection,  cancellationToken, mapper, parameters,transaction))
            .FirstOrDefault();


}