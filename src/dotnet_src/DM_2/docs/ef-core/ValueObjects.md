Коротко: **Value Object** в EF Core мапится двумя путями — как *owned type* (лучше для составных VO) или через *ValueConverter* (лучше для одно-польных VO/ID).

---

# 1) Owned type (OwnsOne / OwnsMany)

Подходит, когда у VO несколько полей или нужна своя таблица/JSON.

### Пример VO

```csharp
public sealed class Name
{
    public string Value { get; }
    public Name(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Name required");
        Value = value;
    }
}
```

### Маппинг в агрегате (в ту же таблицу)

```csharp
builder.OwnsOne(u => u.Name, n =>
{
    n.Property(p => p.Value)
     .HasColumnName("name")
     .HasMaxLength(256)
     .IsRequired();
});
builder.Navigation(u => u.Name).IsRequired(); // чтобы колонка была NOT NULL
```

### В отдельную таблицу (или коллекция VO)

```csharp
// 1:1 в отдельной таблице
builder.OwnsOne(u => u.Email, e =>
{
    e.ToTable("users_email", "identity");
    e.WithOwner().HasForeignKey("user_id");
    e.HasKey("user_id");

    e.Property(p => p.Value).HasColumnName("email").HasMaxLength(100).IsRequired();
});

// 1:many коллекция VO
builder.OwnsMany(u => u.PhoneNumbers, p =>
{
    p.ToTable("user_phones", "identity");
    p.WithOwner().HasForeignKey("user_id");
    p.Property(x => x.Value).HasColumnName("phone").IsRequired();
    p.HasKey("user_id", "phone");
});
```

### В JSON-колонку (EF7+; в EF10 для SQL Server 2025 есть нативный `json`)

```csharp
builder.OwnsOne(u => u.Settings, o =>
{
    o.ToJson(); // провайдер должен поддерживать (SQL Server 2025 / PostgreSQL Npgsql)
});
```

> Иммутабельные VO поддерживаются: конструктор с параметрами + get-only свойства EF может заполнить.

---

# 2) ValueConverter (одно поле)

Идеален для **строго типизированных ID** и простых VO.

### Пример VO/ID

```csharp
public readonly record struct UserId(long Value);
```

### Маппинг

```csharp
builder.Property(u => u.Id)
    .HasConversion(
        toProvider:  (UserId v) => v.Value,
        fromProvider:(long v)    => new UserId(v));

/* Если VO не record/struct и сравнение по ссылке — добавь ValueComparer:
var comparer = new ValueComparer<UserId>(
    (a,b) => a.Value == b.Value,
    v => v.Value.GetHashCode(),
    v => new UserId(v.Value));
builder.Property(u => u.Id).Metadata.SetValueComparer(comparer);
*/
```

Ещё пример (Email как строка):

```csharp
builder.Property(u => u.Email)
    .HasConversion(
        (Email v) => v.Value,
        (string v) => new Email(v))
    .HasMaxLength(100)
    .IsRequired();
```

---

# 3) Nullable / Required нюансы

* Для **reference-VO** укажи обязательность:
  `builder.Navigation(u => u.Email).IsRequired();`
* Для **value-type?** колонка будет `NULL`. Сделай non-nullable и/или `.IsRequired()` если нужно NOT NULL.

---

# Что выбирать

* **1 поле** → `ValueConverter` (проще, быстрее, хранится как примитив).
* **Несколько полей / коллекция / нужна отдельная таблица или JSON** → `OwnsOne/OwnsMany`.
* **Коллекция примитивов** → как массив/JSON (`Property(...).HasColumnType("bigint[]"/"json")`) или как коллекция VO через `OwnsMany` в join-таблицу.

Если пришлёшь конкретный VO (код), дам точный фрагмент конфигурации под твой провайдер (PostgreSQL/SQL Server).
