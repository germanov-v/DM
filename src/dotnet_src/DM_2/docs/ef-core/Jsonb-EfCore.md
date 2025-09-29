Коротко: в Npgsql для EF Core это делается двумя путями — **(A)** «сырой jsonb» (`string`/`JsonDocument` + `HasColumnType("jsonb")`) и **(B)** «типобезопасный JSON-объект» через EF Core JSON-маппинг (`Owned` + `.ToJson()`), где по полям можно писать LINQ. Я покажу оба.

---

# A) Сырой `jsonb` (string/JsonDocument)

### Модель + конфиг

```csharp
public sealed class ExternalIdentity
{
    public long Id { get; init; }
    public long UserId { get; init; }

    // можно string, можно JsonDocument / JsonElement
    public string RawProfileJson { get; init; } = default!;
}

modelBuilder.Entity<ExternalIdentity>(e =>
{
    e.ToTable("external_providers", "identity");
    e.HasKey(x => x.Id);

    e.Property(x => x.RawProfileJson)
     .HasColumnName("raw_profile")
     .HasColumnType("jsonb");
});
```

### Запросы к jsonb

* Достать значение поля (SQL: `#>> '{address,city}'`):

```csharp
var q1 = await db.ExternalIdentities
    .Where(e => EF.Functions.JsonValue(e.RawProfileJson, "{address,city}") == "Moscow")
    .ToListAsync(ct);
```

* Проверка вхождения JSON-фрагмента (SQL: `@>`):

```csharp
var q2 = await db.ExternalIdentities
    .Where(e => EF.Functions.JsonContains(e.RawProfileJson, @"{""is_verified"": true}"))
    .ToListAsync(ct);
```

* Доступ к элементу массива:

```csharp
var q3 = await db.ExternalIdentities
    .Where(e => EF.Functions.JsonValue(e.RawProfileJson, "{roles,0}") == "admin")
    .ToListAsync(ct);
```

> Для производительности добавь индекс:

```sql
CREATE INDEX IF NOT EXISTS ix_external_raw_profile_gin
  ON identity.external_providers
  USING gin (raw_profile jsonb_path_ops);
```

---

# B) Типобезопасный JSON через `.ToJson()` (рекомендуется)

EF Core (с Npgsql) умеет маппить **owned-тип** прямо в `jsonb` и транслировать LINQ в `jsonb`-операторы.

### Модель

```csharp
public sealed class ExternalIdentity
{
    public long Id { get; init; }
    public long UserId { get; init; }
    public ProviderProfile RawProfile { get; init; } = default!;
}

public sealed class ProviderProfile // shape jsonb
{
    public string? Email { get; init; }
    public string? DisplayName { get; init; }
    public string? AvatarUrl { get; init; }

    public Address? Address { get; init; }
    public string[]? Roles { get; init; }
    public bool? IsVerified { get; init; }
}

public sealed class Address
{
    public string? City { get; init; }
    public string? Country { get; init; }
}
```

### Конфигурация

```csharp
modelBuilder.Entity<ExternalIdentity>(e =>
{
    e.ToTable("external_providers", "identity");
    e.HasKey(x => x.Id);

    e.OwnsOne(x => x.RawProfile, json =>
    {
        json.ToJson();                     // <— ключ!
        json.Property(p => p.Email);
        json.OwnsOne(p => p.Address);
        // остальные свойства можно не перечислять — по умолчанию тоже попадут в json
        json.ToJsonProperty("raw_profile"); // колонка в БД
    });
});
```

> Альтернатива записи имени колонки:
>
> * Либо `json.ToJsonProperty("raw_profile")`,
> * Либо у владельца: `e.Property<ProviderProfile>("RawProfile").HasColumnName("raw_profile");`
    >   (в некоторых версиях достаточно `.ToJson()` + имя свойства).

### LINQ по полям JSON (без ручных функций)

```csharp
// email = ...
var byEmail = await db.ExternalIdentities
    .Where(x => x.RawProfile.Email == "test@example.com")
    .ToListAsync(ct);

// вложенное поле
var byCity = await db.ExternalIdentities
    .Where(x => x.RawProfile.Address!.City == "Moscow")
    .ToListAsync(ct);

// по массиву
var byRole = await db.ExternalIdentities
    .Where(x => x.RawProfile.Roles!.Contains("admin"))
    .ToListAsync(ct);

// булево поле
var verified = await db.ExternalIdentities
    .Where(x => x.RawProfile.IsVerified == true)
    .ToListAsync(ct);
```

EF переведёт это в `jsonb`-операторы (`#>>`, `@>`, и т.п.). Индекс GIN, как выше, тоже даст прирост.

---

## Краткий выбор

* Нужно быстро и просто — **A**: `HasColumnType("jsonb")` + `EF.Functions.JsonValue/JsonContains`.
* Хочешь типобезопасность и LINQ по полям — **B**: `Owned` + `.ToJson()` и обращайся к `x.RawProfile.Property`.

Оба подхода ок для C# 14 / .NET 10; главное — убедись, что используешь актуальные пакеты `Npgsql` и `Npgsql.EntityFrameworkCore.PostgreSQL`, и прогнал миграцию для `jsonb` и GIN-индекса.
