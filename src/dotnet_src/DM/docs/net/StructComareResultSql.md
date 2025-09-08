`row == default` не компилится, потому что для **произвольного `struct`** оператор `==` не определён. Компилятор не знает, какой оператор вызывать для обобщённого `TSqlResult`.

Сделай один из двух вариантов.

## Вариант 1 — сравнение через `EqualityComparer`

```csharp
if (EqualityComparer<TSqlResult>.Default.Equals(row, default))
    return null;

return mapper(in row);
```

## Вариант 2 — читать как `Nullable<T>` и проверять на `null`

Это удобнее для value types.

```csharp
public static async Task<TResult?> QuerySingleStructByMapper<TSqlResult, TResult>(
    this IRepository _,
    string sql,
    DbConnection connection,
    CancellationToken cancellationToken,
    MapDbResult<TSqlResult, TResult?> mapper,
    object? parameters = null,
    DbTransaction? transaction = null)
    where TSqlResult : struct
{
    var row = await connection.QueryFirstOrDefaultAsync<TSqlResult?>(
        new CommandDefinition(sql, parameters, transaction: transaction, cancellationToken: cancellationToken));

    if (row is null)
        return null;

    // Важно: 'in' нельзя передавать свойству, нужна переменная
    var value = row.Value;
    return mapper(in value);
}
```

> Примечание: чтобы `mapper(in row)` компилировался, сам делегат должен принимать параметр как `in TSqlResult`:
>
> ```csharp
> public delegate TResult? MapDbResult<TSqlResult, TResult>(in TSqlResult sql)
>     where TSqlResult : struct;
> ```
