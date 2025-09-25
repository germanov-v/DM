Код ок, но есть несколько микро-апгрейдов, если хочется выжать максимум.

## Быстрые выигрыши (без изменения семантики)

1. **Размер словаря заранее**
   Ты ищешь по `email`, значит пользователей вернётся 1.

```csharp
var dict = new Dictionary<long, User>(capacity: 1);
```

2. **Без двойного поиска в Dictionary**
   Вместо `TryGetValue` + `Add` — один проход через `CollectionsMarshal.GetValueRefOrAddDefault`:

```csharp
using System.Runtime.InteropServices;

var cmd = new CommandDefinition(
    sql,
    new { Email = email },
    transaction: _connectionFactory.CurrentTransaction,
    cancellationToken: cancellationToken
);

var rows = await connection.QueryAsync<(long Id, string Name, Guid GuidId, bool IsActive, string Email,
    long RoleId, string RoleAlias, string RoleName, Guid RoleGuid)>(cmd);

var dict = new Dictionary<long, User>(1);

foreach (var item in rows)
{
    ref var userRef = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, item.Id, out bool exists);
    if (!exists)
    {
        userRef = new User(
            new EmailIdentity(item.Email),
            item.IsActive,
            new Name(item.Name),
            null,
            new IdGuid(item.Id, item.GuidId)
        );
        // если у User есть коллекция ролей — можно задать примерную ёмкость
        // userRef.Roles = new List<Role>(4);
    }

    userRef!.AddRole(new Role(new IdGuid(item.RoleId, item.RoleGuid), item.RoleName, item.RoleAlias));
}

return dict.Values;
```

Это убирает лишний хэш-поиск и ветвление.

3. **Избежать копирования большого ValueTuple в foreach**
   ValueTuple — struct; в `foreach (var item in result)` он копируется на каждую итерацию.
   Если хочешь, можно **материализовать в List** и пройтись по **Span** с `ref readonly`:

```csharp
var list = (await connection.QueryAsync<(...)>(cmd)).AsList(); // List<...>
foreach (ref readonly var item in CollectionsMarshal.AsSpan(list))
{
    // тело как выше, только item — by-ref (без копий структуры)
}
```

Профит особенно заметен, если кортеж «толстый» и строк много.

4. **(Опционально) Стриминг вместо буферизации**
   По умолчанию Dapper **буферизует** результат (читает всё в память). Если хочешь читать построчно (меньше пиков по памяти), можно использовать **небуферизованный** запрос. Самый простой путь — **не использовать `.AsList()`** и задать флаг в `CommandDefinition` на «не буферизовать».

```csharp
var cmd = new CommandDefinition(
    sql,
    new { Email = email },
    transaction: _connectionFactory.CurrentTransaction,
    cancellationToken: cancellationToken,
    flags: CommandFlags.None // отключает буферизацию
);
var rows = await connection.QueryAsync<(...)>(cmd);
// важно: перебери rows В ЭТОМ методе, пока соединение/ридер живы
foreach (var item in rows) { ... }
```

> Если данных мало (1 пользователь × N ролей), выигрыш тут микроскопический — можно не трогать.

## Когда стоит менять подход

* **Мульти-мэппинг в Dapper** (`Query<User, Role, User>`) — красиво, но всё равно придётся дедуплицировать `User` в словаре и добавлять роли. Текущий кортежный мэппинг — норм.

* **DTO-класс вместо ValueTuple**: уменьшит копирование в `foreach` (копируется ссылка, не структура), но добавит аллокацию на каждую строку. Для пары десятков строк — не выгодно.

## Вывод

Для твоего кейса «один пользователь по email + несколько ролей»:

* `Dictionary<long, User>(1)`
* `CollectionsMarshal.GetValueRefOrAddDefault`
* (по желанию) `AsList()` + `AsSpan` + `ref readonly` для кортежа

Этого более чем достаточно. Всё остальное — уже микроны.
