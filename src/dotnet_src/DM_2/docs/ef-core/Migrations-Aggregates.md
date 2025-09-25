Крутая схема 👍 Давай разложу на **DDD-агрегаты** и покажу, как это аккуратно замапить в **EF Core**.

---

# 1) Границы агрегатов (DDD)

### Агрегат **User** (Aggregate Root)

* Таблица: `identity.users`
* Внутри агрегата (управляются только через User):

    * **EmailCredential** → `identity.users_email` (1:1)
    * **PhoneCredential** → `identity.users_phone` (1:1)
    * **ExternalIdentities** → `identity.external_providers` (1:many) ← см. замечание по PK ниже
    * **RoleAssignments** → связь с `roles`, но **без навигации на Role**, только `RoleId` (иначе ломаем границы)
* События: `UserConfirmed`, `UserBlocked`, `EmailConfirmed`, и т.п.

### Агрегат **Role** (отдельный)

* Таблица: `identity.roles`
* Управление справочником ролей независимо.
* Связь с пользователем – через **идентификаторы ролей** на стороне User.

### Агрегат **Session** (отдельный)

* Таблица: `identity.sessions`
* Жизненный цикл независим от загрузки User, высокочастотные операции логина/обновления токена — лучше отдельный агрегат.

> Почему так?
>
> * **User** — бизнес-ядро идентификации.
> * **Role** — справочник/политика доступа.
> * **Session** — операционный поток (логины), не должен тянуть граф User.

---

# 2) Модель домена (минимально)

```csharp
public sealed class User : IAggregateRoot
{
    public long Id { get; private set; }
    public Guid GuidId { get; private set; }
    public string Name { get; private set; }

    public bool IsConfirmed { get; private set; }
    public DateTimeOffset ConfirmedChangedAt { get; private set; }

    public bool IsBlocked { get; private set; }
    public DateTimeOffset BlockedChangedAt { get; private set; }
    public int? BlockedReasonCode { get; private set; }
    public string? BlockedReason { get; private set; }

    // 1:1
    public EmailCredential? Email { get; private set; }
    public PhoneCredential? Phone { get; private set; }

    // 1:many
    private readonly List<ExternalIdentity> _externals = new();
    public IReadOnlyCollection<ExternalIdentity> Externals => _externals;

    // роли – только ids (или VO RoleId)
    private readonly HashSet<long> _roleIds = new();
    public IReadOnlyCollection<long> RoleIds => _roleIds;

    // бизнес-методы (подтверждение, блокировка, назначение ролей и т.д.)
}

public sealed class EmailCredential
{
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string PasswordSalt { get; private set; } = default!;
    public bool IsConfirmed { get; private set; }
    public DateTimeOffset? ConfirmedChangedAt { get; private set; }
    public string? ConfirmedCode { get; private set; }
    public DateTimeOffset? ConfirmedCodeExpiresAt { get; private set; }
}

public sealed class PhoneCredential
{
    public string Phone { get; private set; } = default!;
    public bool IsConfirmed { get; private set; }
    public DateTimeOffset? ConfirmedChangedAt { get; private set; }
    public string? ConfirmedCode { get; private set; }
    public DateTimeOffset? ConfirmedCodeExpiresAt { get; private set; }
}

public sealed class ExternalIdentity
{
    // УДОБНО: технический PK (см. заметку по схеме)
    public long Id { get; private set; }

    public string Provider { get; private set; } = default!;
    public string ProviderUserId { get; private set; } = default!;
    public string? EmailFromProvider { get; private set; }
    public string? DisplayName { get; private set; }
    public string? AvatarUrl { get; private set; }

    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTimeOffset? TokenExpiresAt { get; private set; }

    public string? RawProfileJson { get; private set; }
    public bool IsPrimary { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? LastUsedAt { get; private set; }
}
```

---

# 3) Маппинг EF Core

Ключевые приемы:

* **Aggregate Root** → `DbSet<User>`.
* 1:1 таблицы (`users_email`, `users_phone`) → `OwnsOne(...).ToTable(...)` + FK = `user_id`.
* Коллекция `external_providers` → `OwnsMany(...).ToTable(...)` или отдельная сущность с `HasMany(...).WithOne().HasForeignKey("user_id")`. Я покажу `OwnsMany` (подчёркивает принадлежность агрегату).

```csharp
public sealed class IdentityDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>(); // отдельный агрегат
    public DbSet<Session> Sessions => Set<Session>(); // отдельный агрегат

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identity");

        ConfigureUser(modelBuilder);
        ConfigureRole(modelBuilder);
        ConfigureSession(modelBuilder);
    }

    private static void ConfigureUser(ModelBuilder mb)
    {
        mb.Entity<User>(u =>
        {
            u.ToTable("users");
            u.HasKey(x => x.Id);
            u.Property(x => x.GuidId).HasColumnName("guid_id").IsRequired();
            u.Property(x => x.Name).HasColumnName("name").HasMaxLength(250).IsRequired();

            u.Property(x => x.IsConfirmed).HasColumnName("is_confirmed").IsRequired();
            u.Property(x => x.ConfirmedChangedAt).HasColumnName("confirmed_changed_at").IsRequired();

            u.Property(x => x.IsBlocked).HasColumnName("is_blocked").IsRequired();
            u.Property(x => x.BlockedChangedAt).HasColumnName("blocked_changed_at").IsRequired();
            u.Property(x => x.BlockedReasonCode).HasColumnName("blocked_reason_code");
            u.Property(x => x.BlockedReason).HasColumnName("blocked_reason");

            // 1:1 users_email
            u.OwnsOne(x => x.Email, email =>
            {
                email.ToTable("users_email");

                // FK → user_id (PK в таблице email)
                email.WithOwner().HasForeignKey("user_id");
                email.HasKey("user_id");

                email.Property(p => p.Email).HasColumnName("email").HasMaxLength(100).IsRequired();
                email.Property(p => p.PasswordHash).HasColumnName("password_hash").HasMaxLength(200).IsRequired();
                email.Property(p => p.PasswordSalt).HasColumnName("password_salt").HasMaxLength(200).IsRequired();

                email.Property(p => p.IsConfirmed).HasColumnName("is_confirmed").IsRequired();
                email.Property(p => p.ConfirmedChangedAt).HasColumnName("confirmed_changed_at");
                email.Property(p => p.ConfirmedCode).HasColumnName("confirmed_code").HasMaxLength(50);
                email.Property(p => p.ConfirmedCodeExpiresAt).HasColumnName("confirmed_code_expires_at");

                email.HasIndex("email").HasDatabaseName("idx_users_email");
                email.HasIndex("confirmed_code").HasDatabaseName("idx_users_email_confirmed_code")
                    .IncludeProperties(new[] { "is_confirmed", "confirmed_code_expires_at" });
            });

            // 1:1 users_phone
            u.OwnsOne(x => x.Phone, phone =>
            {
                phone.ToTable("users_phone");

                phone.WithOwner().HasForeignKey("user_id");
                phone.HasKey("user_id");

                phone.Property(p => p.Phone).HasColumnName("phone").HasMaxLength(15).IsRequired();
                phone.Property(p => p.IsConfirmed).HasColumnName("is_confirmed").IsRequired();
                phone.Property(p => p.ConfirmedChangedAt).HasColumnName("confirmed_changed_at");
                phone.Property(p => p.ConfirmedCode).HasColumnName("confirmed_code").HasMaxLength(50);
                phone.Property(p => p.ConfirmedCodeExpiresAt).HasColumnName("confirmed_code_expires_at");

                phone.HasIndex("phone").HasDatabaseName("idx_users_phone");
                phone.HasIndex("confirmed_code").HasDatabaseName("idx_users_phone_confirmed_code")
                    .IncludeProperties(new[] { "is_confirmed", "confirmed_code_expires_at" });
            });

            // 1:many external_providers (см. заметку про PK)
            u.OwnsMany(x => x.Externals, ext =>
            {
                ext.ToTable("external_providers");
                ext.WithOwner().HasForeignKey("user_id");

                // НУЖЕН PK строки. Лучше surrogate key:
                ext.HasKey(e => e.Id);
                ext.Property(e => e.Id).HasColumnName("id"); // если добавите колонку id

                ext.Property(e => e.Provider).HasColumnName("provider").HasMaxLength(10).IsRequired();
                ext.Property(e => e.ProviderUserId).HasColumnName("provider_user_id").IsRequired();

                ext.Property(e => e.EmailFromProvider).HasColumnName("email_from_provider");
                ext.Property(e => e.DisplayName).HasColumnName("display_name");
                ext.Property(e => e.AvatarUrl).HasColumnName("avatar_url");

                ext.Property(e => e.AccessToken).HasColumnName("access_token");
                ext.Property(e => e.RefreshToken).HasColumnName("refresh_token");
                ext.Property(e => e.TokenExpiresAt).HasColumnName("token_expires_at");

                ext.Property(e => e.RawProfileJson).HasColumnName("raw_profile");
                ext.Property(e => e.IsPrimary).HasColumnName("is_primary").IsRequired();

                ext.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
                ext.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");
                ext.Property(e => e.LastUsedAt).HasColumnName("last_used_at");

                // индексы
                ext.HasIndex(e => new { e.Provider, e.ProviderUserId })
                   .HasDatabaseName("ux_external_identity_provider_user")
                   .IsUnique();

                // «уникальная основная по провайдеру на пользователя»
                ext.HasIndex("user_id", nameof(ExternalIdentity.Provider))
                   .HasDatabaseName("ux_external_primary_per_user_provider")
                   .IsUnique()
                   .HasFilter("is_primary");
            });

            // роли – без навигации на Role (DDD-границы)
            // Вариант 1: отдельная join-таблица, управляемая User (как часть агрегата)
            u.Metadata.FindNavigation(nameof(User.RoleIds))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);

            // Маппинг join-таблицы можно сделать через Value-collection (EF Core 8/9) или явную сущность UserRole.
            // ПРАКТИЧЕСКИ ПРОЩЕ: явная сущность UserRole (см. ниже альтернативу).
        });
    }

    private static void ConfigureRole(ModelBuilder mb)
    {
        mb.Entity<Role>(r =>
        {
            r.ToTable("roles");
            r.HasKey(x => x.Id);
            r.Property(x => x.GuidId).HasColumnName("guid_id").IsRequired();
            r.Property(x => x.Name).HasColumnName("name").IsRequired();
            r.Property(x => x.Alias).HasColumnName("alias").IsRequired();

            r.HasIndex(x => x.GuidId).HasDatabaseName("idx_sessions_guid_id");
            r.HasIndex(x => x.Name).IsUnique();
            r.HasIndex(x => x.Alias).IsUnique();
        });
    }

    private static void ConfigureSession(ModelBuilder mb)
    {
        mb.Entity<Session>(s =>
        {
            s.ToTable("sessions");
            s.HasKey(x => x.Id);
            s.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            s.Property(x => x.Provider).HasColumnName("provider").IsRequired();
            s.Property(x => x.GuidId).HasColumnName("guid_id").IsRequired();
            s.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            s.Property(x => x.AccessToken).HasColumnName("access_token").IsRequired();
            s.Property(x => x.RefreshToken).HasColumnName("refresh_token").IsRequired();
            s.Property(x => x.RefreshTokenExpiresAt).HasColumnName("refresh_token_expires_at").IsRequired();
            s.Property(x => x.Fingerprint).HasColumnName("fingerprint").IsRequired();
            s.Property(x => x.Ip).HasColumnName("ip");

            s.HasIndex(x => x.GuidId).HasDatabaseName("idx_sessions_guid_id");
            s.HasOne<User>().WithMany() // нет навигации из User, чтобы не тянуть граф
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
```

### Про роли (join-таблица)

В DDD лучше **не держать навигацию User → Role**, а хранить «назначения ролей» как часть агрегата User (только `RoleId`). Технически в EF Core удобней явная сущность:

```csharp
public sealed class UserRole // часть агрегата User
{
    public long UserId { get; private set; }
    public long RoleId { get; private set; }
}

mb.Entity<User>(u =>
{
    u.HasMany<UserRole>()
     .WithOne()
     .HasForeignKey(ur => ur.UserId)
     .OnDelete(DeleteBehavior.Restrict);

    // Можно скрыть DbSet<UserRole> и управлять через методы User.AssignRole(roleId)/RevokeRole(roleId)
});

mb.Entity<UserRole>(ur =>
{
    ur.ToTable("users_roles");
    ur.HasKey(x => new { x.UserId, x.RoleId });
    ur.Property(x => x.UserId).HasColumnName("user_id");
    ur.Property(x => x.RoleId).HasColumnName("role_id");
});
```

Так вы:

* сохраняете границы агрегатов (нет графа на Role),
* используете `users_roles` как техническую таблицу ссылок.

---

# 4) Важная правка в схеме `external_providers`

Сейчас у вас:

```sql
PRIMARY KEY (user_id)
```

и одновременно есть индексы, подразумевающие **много провайдеров** на пользователя. С `PK(user_id)` будет **ровно одна строка** на пользователя — это противоречие.

**Предлагаю**:

* добавить surrogate id:

  ```sql
  ALTER TABLE identity.external_providers
    ADD COLUMN id bigint generated by default as identity PRIMARY KEY;
  ```
* оставить `user_id` как FK + индексы уникальности:

    * `UNIQUE (provider, provider_user_id)`
    * `UNIQUE (user_id, provider) WHERE is_primary`
* или вариант без surrogate: `PRIMARY KEY (user_id, provider)` + `UNIQUE (provider, provider_user_id)`.
  Для EF Core с `OwnsMany` удобнее иметь `id`.

---

# 5) Поведение загрузки/запросов

* Чтение User по email:

    * репозиторий ищет в `users_email` по индексу, берёт `user_id`, грузит агрегат `User` (и при необходимости `Email`, `Phone`, `Externals` через `Include` только в пределах агрегата).
* Изменение пароля/email/phone/externals — только через методы агрегата `User`, транзакция — на уровне агрегата.
* Создание/ревокация сессий — через `SessionRepository`, **без загрузки User** (только валидация по `UserId`).

---

# 6) TL;DR

* **User** — корень агрегата; `users_email`, `users_phone`, `external_providers` — **внутри** агрегата (OwnsOne/OwnsMany, отдельные таблицы).
* **Role** и **Session** — отдельные агрегаты.
* Связь User↔Role — через `users_roles` (явная join-сущность, без навигации на Role в домене User).
* `external_providers` — поправить PK (лучше добавить колонку `id`).

Если хочешь, могу сгенерировать конкретные `EntityTypeConfiguration`-классы и минимальные миграции под это (включая ALTER для `external_providers`).
