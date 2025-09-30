окей, давай по сути: как держать высокую нагрузку на EF Core + PostgreSQL (Npgsql), не уронив БД и не забив пул.

# 1) DbContext: срок жизни и пуллинг

* **Per-request (Scoped)** — дефолт. Один `DbContext` на один web-запрос/юнит работы.
* **Не шэрь `DbContext` между потоками.** Он не thread-safe.
* **Пуллинг контекстов** даёт +к производительности и меньше аллокаций:

  ```csharp
  services.AddNpgsqlDataSource(connString, b =>
  {
      // опционально: подготовка команд, multiplexing и т.п.
  });
  services.AddDbContextPool<AppDbContext>((sp, options) =>
  {
      var dataSource = sp.GetRequiredService<NpgsqlDataSource>();
      options.UseNpgsql(dataSource);
      options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
      options.EnableSensitiveDataLogging(false);
      options.UseModel(CompiledModels.AppDbContextModel.Instance); // если скомпилированная модель
  }, poolSize: 256); // подбери под свой трафик
  ```
* Для **бэкенд-джоб/фона** используй `IDbContextFactory<T>`:

  ```csharp
  services.AddPooledDbContextFactory<AppDbContext>(o => o.UseNpgsql(connString));
  // ...
  await using var db = await factory.CreateDbContextAsync();
  ```

# 2) Подключения: как не упереться в max_connections

* ADO.NET уже делает **connection pooling**. Управляй **параметрами строки подключения**:

    * `MaxPoolSize=200; MinPoolSize=5; Timeout=15;`
* PostgreSQL по умолчанию `max_connections ~ 100–200`. **Сумма активных пулов всех сервисов** не должна его выбивать.
* **Multiplexing (Npgsql)** позволяет нескольким логическим командам делить один физический коннект (сильно снижает нагрузку при большом количестве коротких запросов).
* Держи **время жизни запроса** коротким: не тянуть соединение в долгих транзакциях/стримах.

# 3) Транзакции и ретраи (resiliency)

* **Одна транзакция на юнит работы** (команда/handler). Не тащи её во вью/внешние вызовы.
* Включи **ретраи** на уровне EF:

  ```csharp
  options.UseNpgsql(dataSource, o =>
      o.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null)
  );
  options.CommandTimeout(15); // секунды
  ```
* Для массовых апдейтов — в некоторых случаях лучше **сырой SQL** или специализированные bulk-библиотеки.

# 4) Трекинг, ChangeTracker и батчи

* **Чтение по умолчанию без трекинга**:

  ```csharp
  options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
  // В коде: db.Set<T>().AsNoTracking()
  ```
* Для write-сценариев:

    * При массовых операциях:

      ```csharp
      db.ChangeTracker.AutoDetectChangesEnabled = false;
      // ... AddRange / UpdateRange ...
      await db.SaveChangesAsync(acceptAllChangesOnSuccess: false);
      db.ChangeTracker.AutoDetectChangesEnabled = true;
      ```
* **Батчинг команд** EF Core делает сам; можно ограничить:

  ```csharp
  options.MaxBatchSize(128);
  ```

# 5) Запросы: сплит, проекции, компиляция

* **Проецируй ровно то, что нужно**:

  ```csharp
  var dto = await db.Users
      .Where(x => x.IsActive)
      .Select(x => new UserListItem { Id = x.Id, Name = x.Name })
      .ToListAsync();
  ```
* Для графов с навигациями используй **Split queries** (чтобы не получить Cartesian explosion):

  ```csharp
  db.ChangeTracker.QuerySplittingBehavior = QuerySplittingBehavior.SplitQuery;
  // или на запросе: .AsSplitQuery()
  ```
* **Скомпилированные запросы** под частые горячие запросы:

  ```csharp
  static readonly Func<AppDbContext, Guid, Task<User?>> _getUserByGuid =
      EF.CompileAsyncQuery((AppDbContext db, Guid id) =>
          db.Users.AsNoTracking().FirstOrDefault(u => u.GuidId == id));

  var user = await _getUserByGuid(db, guid);
  ```
* **Скомпилированная модель** (ускоряет startup и метаданные):

  ```
  dotnet ef dbcontext optimize
  ```

  и затем `options.UseModel(CompiledModels.AppDbContextModel.Instance)`.

# 6) Конкурентный доступ

* По умолчанию EF — **оптимистическая конкуренция**. Добавь токен:

  ```csharp
  builder.Property<uint>("xmin").IsRowVersion(); // для PostgreSQL
  ```

  Лови `DbUpdateConcurrencyException` и разруливай конфликты.
* **Пессимистические блокировки** — только когда надо:
  `.FromSql($"SELECT ... FOR UPDATE")` или `db.Database.BeginTransaction()` и сырой SQL.

# 7) Индексы, JSONB и фильтрация

* Создавай **индексы под частые WHERE/ORDER BY**, следи за планами (`EXPLAIN ANALYZE`).
* Для `jsonb` — **GIN индекс** и операторы `@>`, `->>`:

  ```csharp
  builder.Property(x => x.RawProfileJson)
         .HasColumnType("jsonb")
         .HasColumnName("raw_profile");

  // пример запроса
  var q = db.Users.Where(u => EF.Functions.JsonContains(u.RawProfileJson, """{"lang":"ru"}"""));
  ```

  (или `.FromSqlInterpolated` для сложных условий)

# 8) Настройки Npgsql, которые помогают

* **Prepared statements** (Npgsql включает/оптимизирует автоматически; под высокой нагрузкой полезно).
* **Multiplexing** — хорошо для микросервисов с множеством быстрых SELECTов.
* **TCP keepalive**/timeouts в k8s/Cloud — чтобы не висели мёртвые коннекты.

# 9) Лимиты пула vs степень параллелизма

* Если у тебя `Kestrel`/воркеры могут держать, скажем, до **N** одновременно активных EF-операций, ставь `MaxPoolSize >= N`, но **держи запас** к `max_connections` на стороне Postgres (репликация, админ-сессии и т.д.).
* Не плодить `Parallel.ForEach` с EF внутри. Лучше **очереди/каналы** и контролируемый `DegreeOfParallelism`.

# 10) Логи, метрики, профилинг

* В проде урежь EF-логи:

  ```csharp
  options.LogTo(_ => { }, LogLevel.Warning);
  ```
* Включи **Application Insights / OpenTelemetry** + **пул коннектов/команд** метрики (в Npgsql есть counters).
* Для “медленных” запросов — логируй > X мс; смотри планы в PostgreSQL.

---

### Быстрый шаблон настройки для прод

```csharp
services.AddNpgsqlDataSource(connString, b =>
{
    // b.EnableParameterLogging(); // только в dev
    // b.UseMappedEnum(...) // если есть enum
});

services.AddDbContextPool<AppDbContext>((sp, opt) =>
{
    var ds = sp.GetRequiredService<NpgsqlDataSource>();
    opt.UseNpgsql(ds, o => o.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null))
       .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
       .CommandTimeout(15);
    opt.UseModel(CompiledModels.AppDbContextModel.Instance);
    opt.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
}, poolSize: 256);
```

Если покажешь свой типичный «тяжёлый» запрос/сценарий нагрузки, я подскажу точечно: где сплит/проекция, где компиляция запроса, и какие индексы на `jsonb` лучше поставить.
