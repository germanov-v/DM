
```csharp

using System.Collections.Concurrent;
using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

public sealed class PgDbConnectionFactory<TConnection> : IPgConnectionFactory<TConnection>
    where TConnection : DbConnection
{
    private readonly DbConnectionOptions _options;
    private readonly ILogger<PgDbConnectionFactory<TConnection>> _logger;

    // Пулы соединений по алиасам
    private readonly ConcurrentDictionary<string, NpgsqlDataSource> _sources = new();

    // Состояние текущего async-контекста (per job / per logical call chain)
    private readonly AsyncLocal<PerFlowState?> _state = new();

    private bool _disposed;

    public PgDbConnectionFactory(
        IOptions<DbConnectionOptions> options,
        ILogger<PgDbConnectionFactory<TConnection>> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger  = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // === IPgConnectionFactory<TConnection> ===

    public DbTransaction? CurrentTransaction => _state.Value?.Transaction;

    public async Task<DbConnection> OpenAsync(string alias, CancellationToken ct)
    {
        ThrowIfDisposed();
        var ds = GetOrCreateDataSource(alias);
        // Берем открытое подключение из пула
        var conn = await ds.OpenConnectionAsync(ct);
        return conn;
    }

    public async Task TransactionStart(
        CancellationToken cancellationToken,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        ThrowIfDisposed();

        var s = _state.Value;
        if (s?.Transaction is not null)
            throw new InvalidOperationException("Active transaction already exists in this context.");

        // Выбираем алиас для транзакции:
        // 1) если ранее уже открыт "контекстный" коннект — используем его;
        // 2) иначе используем дефолтный алиас из настроек (например, DbApiConnection).
        var alias = s?.Alias ?? nameof(DbConnectionOptions.DbApiConnection);

        var ds = GetOrCreateDataSource(alias);
        var conn = await ds.OpenConnectionAsync(cancellationToken); // NpgsqlConnection : DbConnection
        DbTransaction? tx = null;
        try
        {
            tx = await conn.BeginTransactionAsync(isolationLevel, cancellationToken);
            _state.Value = new PerFlowState(alias, conn, tx);
        }
        catch
        {
            if (tx is not null) await tx.DisposeAsync();
            await conn.DisposeAsync();
            throw;
        }
    }

    public async Task TransactionCommit(CancellationToken cancellationToken)
    {
        var s = _state.Value ?? throw new InvalidOperationException("No active transaction.");
        try
        {
            await s.Transaction!.CommitAsync(cancellationToken);
        }
        finally
        {
            await CleanupStateAsync();
        }
    }

    public async Task TransactionRollback(CancellationToken cancellationToken)
    {
        var s = _state.Value ?? throw new InvalidOperationException("No active transaction.");
        try
        {
            await s.Transaction!.RollbackAsync(cancellationToken);
        }
        finally
        {
            await CleanupStateAsync();
        }
    }

    public async Task CloseConnections(CancellationToken cancellationToken)
    {
        // Закрываем "контекстное" соединение/транзакцию, если вдруг висят
        await CleanupStateAsync();
    }

    // === Внутр. реализация ===

    private NpgsqlDataSource GetOrCreateDataSource(string alias)
    {
        return _sources.GetOrAdd(alias, static (key, state) =>
        {
            var (opts, logger) = state;
            var cs = ResolveConnectionString(opts, key);
            var b  = new NpgsqlDataSourceBuilder(cs);
            // ваши маппинги/настройки
            b.UseJsonNet();
            var ds = b.Build();
            logger.LogInformation("NpgsqlDataSource created for alias: {Alias}", key);
            return ds;
        }, (_options, _logger));
    }

    private static string ResolveConnectionString(DbConnectionOptions o, string alias)
    {
        if (o.Connections is not null && o.Connections.TryGetValue(alias, out var cs))
            return cs;

        return alias switch
        {
            nameof(DbConnectionOptions.DbApiConnection)     => o.DbApiConnection
                ?? throw new InvalidOperationException($"Connection string is null for {alias}"),
            nameof(DbConnectionOptions.DDDF)   => o.DDDF
                ?? throw new InvalidOperationException($"Connection string is null for {alias}"),
            _ => throw new ArgumentOutOfRangeException(nameof(alias), alias, "Unknown DB alias"),
        };
    }

    private async Task CleanupStateAsync()
    {
        var s = _state.Value;
        _state.Value = null;

        if (s is null) return;

        try
        {
            if (s.Transaction is not null)
                await s.Transaction.DisposeAsync();
        }
        finally
        {
            if (s.Connection is not null)
                await s.Connection.DisposeAsync();
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(GetType().Name);
    }

    // Контейнер для состояния текущего async-потока
    private sealed class PerFlowState
    {
        public string Alias { get; }
        public DbConnection Connection { get; }
        public DbTransaction Transaction { get; }

        public PerFlowState(string alias, DbConnection connection, DbTransaction transaction)
        {
            Alias = alias;
            Connection = connection;
            Transaction = transaction;
        }
    }

    // === Dispose ===

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // Чистим текущее состояние
        _ = CleanupStateAsync().AsTask();

        foreach (var ds in _sources.Values)
        {
            try { ds.Dispose(); } catch { /* ignore */ }
        }
        _sources.Clear();

        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        await CleanupStateAsync();

        foreach (var ds in _sources.Values)
        {
            try { await ds.DisposeAsync(); } catch { /* ignore */ }
        }
        _sources.Clear();

        GC.SuppressFinalize(this);
    }
} 
```