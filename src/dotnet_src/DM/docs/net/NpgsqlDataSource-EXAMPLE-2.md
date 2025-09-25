Отличный вопрос. Коротко: **NpgsqlDataSource — это фабрика + пул/конфиг соединений**, но он не «ломает» транзакции. Если вам нужна **одна транзакция на весь запрос (scope)**, вы всё так же можете **взять одно физическое соединение у DataSource и держать его открытым** до коммита/роллбэка. Главное — **получать соединение из `NpgsqlDataSource`**, а не создавать `new NpgsqlConnection(...)`.

Ниже — практичный способ эволюции вашего кода под `NpgsqlDataSource`.

---

# Что важно знать про DataSource

1. **Жизненный цикл**

    * `NpgsqlDataSource` создаётся 1 раз (обычно `Singleton` в DI) и живёт всё приложение.
    * Соединения берём через `dataSource.OpenConnectionAsync()` / `CreateCommand()`.

2. **Транзакции и мультиплексирование**

    * Если вы включаете Multiplexing, **явные транзакции/`NpgsqlTransaction` недоступны**. Для UoW оставьте `Multiplexing=false` (по умолчанию так и есть).
    * Для длительных транзакций держите **одно открытое соединение** в scope.

3. **Два режима доступа** (совмещайте по необходимости):

    * **Без транзакции**: на короткие операции — открыли соединение из DataSource, сделали запрос, закрыли (пусть пул работает).
    * **С транзакцией (UoW)**: в начале scope — открыть одно соединение, начать транзакцию, весь код в этом scope берёт **то же** соединение, в конце — commit/rollback и закрыть.

---

# Рефакторинг вашего `PgsqlConnectionFactory`

Идея: хранить в экземпляре (Scoped) **текущее соединение и транзакцию**; при активной транзакции любые запросы берут **это** соединение, иначе — берут «временное» из DataSource.

Ключевые изменения:

* Внедряем `NpgsqlDataSource` (Singleton).
* Убираем создание `new NpgsqlConnection(...)` — только `dataSource.OpenConnectionAsync()`.
* Упрощаем синхронизацию: на уровне web-запроса обычно достаточно scoped-объекта (без глобального `SemaphoreSlim`), но если есть параллелизм внутри scope — оставьте лок.
* Исправляем `Commit/Rollback` сообщения и аккуратно освобождаем ресурсы.
* `CloseConnections` делает `Dispose` только если мы владеем текущим соединением.

```csharp
public sealed class PgsqlConnectionFactory : IConnectionFactory<NpgsqlConnection>, IAsyncDisposable, IDisposable
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<PgsqlConnectionFactory> _logger;

    private readonly SemaphoreSlim _lock = new(1, 1);
    private NpgsqlConnection? _currentConnection;   // активное соединение в рамках транзакции/скоупа
    private NpgsqlTransaction? _currentTx;
    private bool _disposed;

    public PgsqlConnectionFactory(NpgsqlDataSource dataSource, ILogger<PgsqlConnectionFactory> logger)
    {
        _dataSource = dataSource;
        _logger = logger;
    }

    // Если есть активная транзакция — возвращаем прикреплённое к ней соединение,
    // иначе открываем короткоживущее соединение для разовой операции.
    public async Task<NpgsqlConnection> GetCurrentConnection(CancellationToken ct)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(PgsqlConnectionFactory));

        // Быстрый путь: в транзакции уже есть соединение
        if (_currentTx is not null && _currentConnection is not null)
            return _currentConnection;

        // Разовая операция: берём соединение из DataSource (владеет вызывающий)
        var conn = await _dataSource.OpenConnectionAsync(ct);
        return conn;
    }

    // Начать UoW-транзакцию. Берём одно соединение у DataSource и держим до Commit/Rollback.
    public async ValueTask InitialTransaction(CancellationToken ct, IsolationLevel iso = IsolationLevel.ReadCommitted)
    {
        if (_disposed) throw new InvalidOperationException("Factory disposed");
        if (_currentTx is not null) return;

        await _lock.WaitAsync(ct);
        try
        {
            if (_currentTx is not null) return;

            _currentConnection ??= await _dataSource.OpenConnectionAsync(ct);
            _currentTx = await _currentConnection.BeginTransactionAsync(iso, ct);
        }
        finally
        {
            _lock.Release();
        }
    }

    public DbTransaction? CurrentTransaction => _currentTx;

    public void SetTransaction(DbTransaction? tx)
    {
        _currentTx = (NpgsqlTransaction?)tx;
        if (_currentTx is not null)
            _currentConnection = (NpgsqlConnection)_currentTx.Connection!;
    }

    public async Task CommitTransaction(CancellationToken ct)
    {
        if (_currentTx is null) throw new InvalidOperationException("Commit failed: transaction is not active.");

        await _lock.WaitAsync(ct);
        try
        {
            if (_currentTx is null) return;
            await _currentTx.CommitAsync(ct);
            await _currentTx.DisposeAsync();
            _currentTx = null;

            // Закрываем соединение, так как мы его «удерживали» ради транзакции
            if (_currentConnection is not null)
            {
                await _currentConnection.DisposeAsync();
                _currentConnection = null;
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task RollbackTransaction(CancellationToken ct)
    {
        if (_currentTx is null) throw new InvalidOperationException("Rollback failed: transaction is not active.");

        await _lock.WaitAsync(ct);
        try
        {
            if (_currentTx is null) return;
            await _currentTx.RollbackAsync(ct);
            await _currentTx.DisposeAsync();
            _currentTx = null;

            if (_currentConnection is not null)
            {
                await _currentConnection.DisposeAsync();
                _currentConnection = null;
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    // Явительно закрыть удерживаемое соединение (если нет транзакции)
    public async Task CloseConnections(CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            if (_currentTx is not null) return; // в транзакции не закрываем
            if (_currentConnection is not null)
            {
                await _currentConnection.DisposeAsync();
                _currentConnection = null;
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        if (_currentTx is not null)
            await _currentTx.DisposeAsync();

        if (_currentConnection is not null)
            await _currentConnection.DisposeAsync();

        _lock.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _currentTx?.Dispose();
        _currentConnection?.Dispose();
        _lock.Dispose();
        GC.SuppressFinalize(this);
    }
}
```

### Как вызывать

* **Без транзакции** (короткий запрос):

  ```csharp
  await using var conn = await factory.GetCurrentConnection(ct);
  await using var cmd = conn.CreateCommand();
  cmd.CommandText = "...";
  using var reader = await cmd.ExecuteReaderAsync(ct);
  ```

* **В транзакции (в начале запроса/scoped pipeline middleware/фасаде оркестратора):**

  ```csharp
  await factory.InitialTransaction(ct);
  try
  {
      var conn = await factory.GetCurrentConnection(ct); // вернётся то же соединение
      await using var cmd1 = conn.CreateCommand();
      cmd1.Transaction = (NpgsqlTransaction?)factory.CurrentTransaction;
      // ... несколько команд ...
      await factory.CommitTransaction(ct);
  }
  catch
  {
      await factory.RollbackTransaction(ct);
      throw;
  }
  ```

> Заметьте: если транзакция активна, **не** делайте `await using var conn = await factory.GetCurrentConnection(ct)` — вы не владеете временем жизни этого соединения (оно принадлежит фабрике до коммита/роллбэка). Просто возьмите ссылку и создавайте команды с `Transaction`.

---

## DI и конфигурация

* Регистрируйте DataSource как **Singleton**:

  ```csharp
  services.AddSingleton(sp =>
  {
      var cs = configuration.GetConnectionString("Default");
      var builder = new NpgsqlDataSourceBuilder(cs);
      // ВАЖНО: для явных транзакций оставьте Multiplexing выключенным (по умолчанию).
      // builder.EnableMultiplexing(); // НЕ включайте при UoW
      return builder.Build();
  });

  services.AddScoped<IConnectionFactory<NpgsqlConnection>, PgsqlConnectionFactory>();
  ```

* Если вам принципиально нужен `TransactionScope`, убедитесь что:

    * `Enlist=true` в строке подключения (по умолчанию true),
    * используете `TransactionScopeAsyncFlowOption.Enabled`,
    * понимаете, что это ambient-подход (я бы предпочёл явные `Begin/Commit` в UoW).

---

## Замечания к вашему исходнику

* В `RollbackTransaction` сообщение об ошибке копирует текст от Commit — поправлено выше.
* `CloseConnections` у вас делал `DisposeAsync` без проверки txn — добавил защиту.
* Нет смысла хранить `DbConnectionOptions.ConnectionString` — теперь источник строк в `NpgsqlDataSource`.
* Повторные ретраи открытия (`Polly`) теперь обычно не нужны: открытие идёт из пула; если всё же хотите — оборачивайте только **краткоживущие** открытия (не те, что в транзакции). Как правило, лучше настроить пул и лимиты соединений на стороне PostgreSQL.

---

## Итог

* `NpgsqlDataSource` полностью совместим с концепцией «**одно соединение на scope + транзакция**».
* Просто перестаньте создавать `NpgsqlConnection` вручную, держите «текущее» соединение только когда есть активная транзакция, и отдавайте краткоживущие соединения для одиночных операций.
* Не включайте Multiplexing, если нужны явные транзакции/UoW.
