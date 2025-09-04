using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using Core.Domain.SharedKernel.Abstractions;
using Dapper;

namespace Core.Infrastructure.Persistence.Repositories;

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


    public static async Task<IReadOnlyList<TResult>> QueryMapperList<TSqlResult, TResult>(
        this IRepository _,
        string sql,
        DbConnection connection,
        CancellationToken cancellationToken,
        Action<TSqlResult, Dictionary<long, TResult>> mapper,
        object? parameters = null,
        DbTransaction? transaction = null)
        where TSqlResult : struct

    {
        var rows = await connection.QueryAsync<TSqlResult>(
            new CommandDefinition(sql,
                parameters,
                transaction: transaction,
                cancellationToken: cancellationToken)
        );

        var lookup = new Dictionary<long, TResult>();

        foreach (var row in rows)
        {
            mapper(row, lookup);
        }

        return lookup.Values.ToList();
    }

    public static async Task<TResult?> QueryMapperFirst<TSqlResult, TResult>(
        this IRepository repository,
        string sql,
        DbConnection connection,
        CancellationToken cancellationToken,
        Action<TSqlResult, Dictionary<long, TResult>> mapper,
        object? parameters = null,
        DbTransaction? transaction = null)
        where TSqlResult : struct
        => (await QueryMapperList(repository, sql, connection, cancellationToken, mapper, parameters, transaction))
            .FirstOrDefault();
}