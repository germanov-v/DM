ок, вот минимальный «адаптер», который даёт настоящий `IAsyncEnumerable<T>` поверх Dapper — с чтением по `ReadAsync` и маппингом через dapper-парсер.

```csharp
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;
using Dapper;

public static class DapperAsyncStream
{
    public static async IAsyncEnumerable<T> QueryAsyncEnumerable<T>(
        this DbConnection connection,
        string sql,
        object? parameters = null,
        DbTransaction? transaction = null,
        CommandBehavior behavior = CommandBehavior.SequentialAccess,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var cmd = new CommandDefinition(
            sql,
            parameters,
            transaction: transaction,
            cancellationToken: cancellationToken,
            flags: CommandFlags.None // важно: без буферизации
        );

        // держим reader и соединение открытыми до конца enumerate
        using var reader = await connection.ExecuteReaderAsync(cmd, behavior);
        // разово получаем парсер от Dapper (очень быстрый делегат) 
        var parser = reader.GetRowParser<T>();

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return parser(reader);
        }
    }
}
```

### Как использовать

```csharp
await foreach (var row in connection.QueryAsyncEnumerable<(long Id, string Name)>(
                   "select id, name from users where active = @flag",
                   new { flag = true },
                   cancellationToken: ct))
{
    Console.WriteLine($"{row.Id} {row.Name}");
}
```

### Пояснения и тонкости

* `CommandFlags.None` → Dapper отдаст ленивое чтение, мы сами крутим `DbDataReader.ReadAsync`.
* `reader.GetRowParser<T>()` — это dapper-расщепитель, который один раз строится по схеме ридера и дальше очень быстро маппит строки в `T` (в т.ч. в `ValueTuple`, записи, классы).
* `CommandBehavior.SequentialAccess` снижает память при чтении больших полей (стримы/VARBINARY/TEXT).
* Важно: соединение/ридер живут пока идёт `await foreach`. Не «утекай» энумератор наружу после закрытия `connection`.
* Если нужен **маппинг на лету**, добавь перегрузку с `Func<TSql, TResult>`:

```csharp
public static async IAsyncEnumerable<TResult> QueryAsyncEnumerable<TSql, TResult>(
    this DbConnection connection,
    string sql,
    Func<TSql, TResult> map,
    object? parameters = null,
    DbTransaction? transaction = null,
    CommandBehavior behavior = CommandBehavior.SequentialAccess,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    var cmd = new CommandDefinition(sql, parameters, transaction, cancellationToken, flags: CommandFlags.None);

    using var reader = await connection.ExecuteReaderAsync(cmd, behavior);
    var parser = reader.GetRowParser<TSql>();

    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        yield return map(parser(reader));
}
```

### Стриминг с агрегацией 1→N (по ключу, без удержания всего набора)

Если в SQL сделать `ORDER BY` по ключу агрегата, можно собирать агрегаты «на потоке»:

```csharp
public static async IAsyncEnumerable<TAgg> QueryAggregateStream<TRow, TKey, TAgg>(
    this DbConnection connection,
    string sql,
    Func<TRow, TKey> keyOf,
    Func<TRow, TAgg> createAgg,
    Action<TAgg, TRow> addToAgg,
    object? parameters = null,
    DbTransaction? transaction = null,
    CommandBehavior behavior = CommandBehavior.SequentialAccess,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
    where TKey : IEquatable<TKey>
{
    var cmd = new CommandDefinition(sql, parameters, transaction, cancellationToken, CommandFlags.None);
    using var reader = await connection.ExecuteReaderAsync(cmd, behavior);
    var parse = reader.GetRowParser<TRow>();

    var has = false;
    TKey? kcur = default;
    TAgg? agg = default;

    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
    {
        var row = parse(reader);
        var k = keyOf(row);

        if (!has || !k.Equals(kcur!))
        {
            if (has) yield return agg!;
            kcur = k;
            agg = createAgg(row);
            has = true;
        }
        else
        {
            addToAgg(agg!, row);
        }
    }
    if (has) yield return agg!;
}
```

Использование (пример `Blog` ← `Post`):

```csharp
await foreach (var blog in connection.QueryAggregateStream(
                   sql: @"SELECT b.Id, b.Title, p.Id, p.Title 
                          FROM Blogs b LEFT JOIN Posts p ON p.BlogId=b.Id
                          WHERE b.OwnerId=@owner ORDER BY b.Id",
                   keyOf: ( (long BlogId, string BlogTitle, long? PostId, string? PostTitle) r ) => r.BlogId,
                   createAgg: r => new Blog(r.BlogId, r.BlogTitle, new List<Post>()),
                   addToAgg:  (b, r) => { if (r.PostId is long id) b.Posts.Add(new Post(id, r.PostTitle!)); },
                   parameters: new { owner = userId },
                   cancellationToken: ct))
{
    // обрабатывай блог по мере готовности
}
```

Если нужно — накину версии с `DbTransaction`, тайм-аутами и преднастройкой `CommandBehavior` под SQL Server / Npgsql.
