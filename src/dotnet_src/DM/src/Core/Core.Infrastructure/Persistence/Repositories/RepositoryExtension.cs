using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Core.Domain.SharedKernel.Repositories;
using Dapper;

namespace Core.Infrastructure.Persistence.Repositories;



//public delegate TResult MapDbResult<TSqlResult, TResult>(ref TSqlResult sqlResult, TResult result);
public delegate TResult MapDbResult<TSqlResult, out TResult>(in TSqlResult sqlResult)
//    where TSqlResult:  struct
    ;

public delegate void MapDbResultRef<TSqlResult, TResult>(in TSqlResult sqlResult, ref TResult result)
    //  where TSqlResult:  struct 
    ;

[Obsolete]
public static class RepositoryExtension
{
    public static async IAsyncEnumerable<TResult> QueryStream<TSqlResult, TKey, TResult>(
        this IRepository _,
        string sql,
        Func<TSqlResult, TKey> keyOf,
        Func<TSqlResult, TResult> createResult,
        Action<TSqlResult, TResult> addToResult,
        DbConnection connection,
        [EnumeratorCancellation] CancellationToken cancellationToken,
        object? parameters = null,
        DbTransaction? transaction = null,
        CommandBehavior behavior = CommandBehavior.SequentialAccess
    )
        where TKey : IEquatable<TKey>
        where TResult : notnull
    {
        var cmd = new CommandDefinition(sql, parameters, transaction,
            flags: CommandFlags.None);
    
        await using var reader = await connection.ExecuteReaderAsync(cmd, behavior);
    
        var parse = reader.GetRowParser<TSqlResult>();
    
    
        var has = false;
        TKey? key = default;
        TResult? result = default;
    
        while (await reader.ReadAsync(cancellationToken))
        {
            var row = parse(reader);
            var currentKey = keyOf(row);
    
            if (!has || !currentKey.Equals(key))
            {
                if (has) yield return result ?? throw new InvalidOperationException("Result is null");
                key = currentKey;
                has = true;
                result = createResult(row);
            }
            else
            {
                addToResult(row, result!);
            }
        }
    
        if (has)
            yield return result ?? throw new InvalidOperationException("Result is null");
    }
    
    
    public static async Task<TResult?> QuerySingleByMapper<TSqlResult, TResult>(
            this IRepository _,
            string sql,
            DbConnection connection,
            CancellationToken cancellationToken,
            MapDbResult<TSqlResult, TResult?> mapper,
            object? parameters = null,
            DbTransaction? transaction = null)
        // where TSqlResult : struct
        where TResult : class
    {
        var row = await connection.QueryFirstOrDefaultAsync<TSqlResult?>(
            new CommandDefinition(sql,
                parameters,
                transaction: transaction,
                cancellationToken: cancellationToken)
        );
    
    
        if (row is null)
            return null;
    
    
        return mapper(in row);
    }
    
    public static async Task<IReadOnlyList<TResult>> QueryMultiByMapper<TSqlResult, TResult, TKey>(
            this IRepository _,
            string sql,
            DbConnection connection,
            CancellationToken cancellationToken,
            // Action<TSqlResult, Dictionary<TKey, TResult>> mapper,
            MapDbResultRef<TSqlResult, Dictionary<TKey, TResult>> mapper,
            object? parameters = null,
            DbTransaction? transaction = null)
          where TSqlResult : class?
        where TKey : IEquatable<TKey>
    
    {
        var rows = (await connection.QueryAsync<TSqlResult>(
            new CommandDefinition(sql,
                parameters,
                transaction: transaction,
                cancellationToken: cancellationToken)
        )).AsList();
    
        var lookup = new Dictionary<TKey, TResult>();
    
        // foreach (var row in rows)
        // {
        //      mapper(in row, ref lookup);
        // }
    
        // var list = rows;
        var span = CollectionsMarshal.AsSpan(rows);
        foreach (ref readonly var item in span)
            if (item is not null)
                mapper(in item, ref lookup);
    
        return lookup.Values.ToList();
    }
    
    public static async Task<TResult?> QueryMultiByMapperFirst<TSqlResult, TResult, TKey>(
        this IRepository repository,
        string sql,
        DbConnection connection,
        CancellationToken cancellationToken,
        //  Action<TSqlResult, Dictionary<TKey, TResult>> mapper,
        MapDbResultRef<TSqlResult, Dictionary<TKey, TResult>> mapper,
        object? parameters = null,
        DbTransaction? transaction = null)
        where TSqlResult : class
        where TKey : IEquatable<TKey>
        => (await QueryMultiByMapper(repository, sql, connection, cancellationToken, mapper, parameters, transaction))
            .FirstOrDefault();
}