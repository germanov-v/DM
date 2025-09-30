ок, вот минимальный «пакет» UoW под EF Core + Npgsql, который можно кинуть в свой `Infrastructure` проект и подключить в DI. Включает:

* `IUnitOfWork` + вложенные транзакции (savepoints)
* `ITransactionScope` (Begin/Commit с авто-rollback в `Dispose`)
* Обёртку `ExecuteWithRetryAsync` на базе `ExecutionStrategy`
* DI-регистрацию
* Пример репозитория и хэндлера
* Мини-тесты (xUnit) на вложенность и авто-rollback

---

# Файлы

## 1) Abstractions

```csharp
// File: Abstractions/IUnitOfWork.cs
using System.Threading;
using System.Threading.Tasks;

namespace App.Infrastructure.Uow;

public interface IUnitOfWork
{
    Task<ITransactionScope> BeginAsync(CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    bool HasActiveTransaction { get; }
}

public interface ITransactionScope : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);
}
```

## 2) Реализация для EF Core (с savepoints)

```csharp
// File: Ef/EfUnitOfWork.cs
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace App.Infrastructure.Uow;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly DbContext _db;
    private IDbContextTransaction? _tx;
    private int _depth = 0;

    public EfUnitOfWork(DbContext db) => _db = db;
    public bool HasActiveTransaction => _tx is not null;

    public async Task<ITransactionScope> BeginAsync(CancellationToken ct = default)
    {
        _depth++;

        if (_tx is null)
        {
            _tx = await _db.Database.BeginTransactionAsync(ct);
            return new RootScope(this);
        }
        else
        {
            var name = SavepointName(_depth);
            await _db.Database.CurrentTransaction!.CreateSavepointAsync(name, ct);
            return new NestedScope(this, name);
        }
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    private static string SavepointName(int depth) => $"sp_{depth}";

    private sealed class RootScope : ITransactionScope
    {
        private readonly EfUnitOfWork _uow;
        private bool _committed;

        public RootScope(EfUnitOfWork uow) => _uow = uow;

        public async Task CommitAsync(CancellationToken ct = default)
        {
            if (_uow._tx is null) return;
            await _uow._tx.CommitAsync(ct);
            _committed = true;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (!_committed && _uow._tx is not null)
                    await _uow._tx.RollbackAsync();
            }
            finally
            {
                if (_uow._tx is not null)
                    await _uow._tx.DisposeAsync();

                _uow._tx = null;
                _uow._depth = 0;
            }
        }
    }

    private sealed class NestedScope : ITransactionScope
    {
        private readonly EfUnitOfWork _uow;
        private readonly string _savepoint;
        private bool _committed;

        public NestedScope(EfUnitOfWork uow, string savepoint)
            => (_uow, _savepoint) = (uow, savepoint);

        public async Task CommitAsync(CancellationToken ct = default)
        {
            if (_uow._tx is null) return;
            await _uow._tx.ReleaseSavepointAsync(_savepoint, ct);
            _committed = true;
        }

        public async ValueTask DisposeAsync()
        {
            _uow._depth--;
            if (_uow._tx is null) return;

            if (!_committed)
                await _uow._tx.RollbackToSavepointAsync(_savepoint);
        }
    }
}
```

## 3) Обёртка с ретраями (опционально)

```csharp
// File: Ef/UnitOfWorkExtensions.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Uow;

public static class UnitOfWorkExtensions
{
    /// Выполняет блок с учётом ExecutionStrategy (ретраи транзиентных ошибок).
    /// ВАЖНО: транзакция создаётся внутри делегата, иначе ретраи не сработают правильно.
    public static async Task ExecuteWithRetryAsync(
        this IUnitOfWork uow,
        DbContext db, 
        Func<CancellationToken, Task> work,
        CancellationToken ct = default)
    {
        var strategy = db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await uow.BeginAsync(ct);
            await work(ct);
            await uow.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        });
    }
}
```

## 4) DI-регистрация

```csharp
// File: Di/ServiceCollectionExtensions.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace App.Infrastructure.Uow;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEfUnitOfWork<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        // DbContext уже зарегистрирован где-то выше (AddDbContextPool)
        services.AddScoped<IUnitOfWork, EfUnitOfWork>(sp =>
        {
            var db = (DbContext)sp.GetRequiredService<TDbContext>();
            return new EfUnitOfWork(db);
        });

        return services;
    }
}
```

В `Program.cs`:

```csharp
services.AddDbContextPool<AppDbContext>((sp, opt) =>
{
    var ds = sp.GetRequiredService<Npgsql.NpgsqlDataSource>();
    opt.UseNpgsql(ds, o => o.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null))
       .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
       .CommandTimeout(15);
    opt.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    // opt.UseModel(CompiledModels.AppDbContextModel.Instance); // если используешь
}, poolSize: 256);

services.AddEfUnitOfWork<AppDbContext>();
```

## 5) Пример репозитория (без знания о транзакциях)

```csharp
// File: Repositories/UserRepository.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Data;

public interface IUserRepository
{
    Task<User?> GetAsync(long id, CancellationToken ct);
    Task<User?> GetForUpdateAsync(long id, CancellationToken ct);
    void Add(User user);
}

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;

    public Task<User?> GetAsync(long id, CancellationToken ct) =>
        _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<User?> GetForUpdateAsync(long id, CancellationToken ct) =>
        _db.Users
           .FromSqlInterpolated($@"SELECT * FROM users WHERE id = {id} FOR NO KEY UPDATE")
           .FirstOrDefaultAsync(ct);

    public void Add(User user) => _db.Users.Add(user);
}
```

## 6) Пример хэндлера

```csharp
// File: Handlers/CreateOrderHandler.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using App.Infrastructure.Uow;

namespace App.Application.Handlers;

public sealed class CreateOrderHandler
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRepository _users;
    private readonly IOrderRepository _orders;
    private readonly AppDbContext _db; // для ExecuteWithRetryAsync

    public CreateOrderHandler(IUnitOfWork uow, IUserRepository users, IOrderRepository orders, AppDbContext db)
        => (_uow, _users, _orders, _db) = (uow, users, orders, db);

    public async Task Handle(long userId, CancellationToken ct)
    {
        // Вариант 1: ручные Begin/Commit
        await using (var tx = await _uow.BeginAsync(ct))
        {
            var user = await _users.GetForUpdateAsync(userId, ct);
            if (user is null) throw new InvalidOperationException("User not found");

            _orders.Add(new Order(user.Id));
            await _uow.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }

        // Вариант 2: обёртка с ретраями (где нужно)
        // await _uow.ExecuteWithRetryAsync(_db, async ct2 =>
        // {
        //     var user = await _users.GetForUpdateAsync(userId, ct2);
        //     _orders.Add(new Order(user!.Id));
        // }, ct);
    }
}
```

## 7) Тесты (xUnit, минимальные)

```csharp
// File: Tests/EfUnitOfWorkTests.cs
using System;
using System.Threading.Tasks;
using App.Infrastructure.Uow;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class EfUnitOfWorkTests
{
    private sealed class TestDb : DbContext
    {
        public TestDb(DbContextOptions<TestDb> options) : base(options) { }
        public DbSet<Item> Items => Set<Item>();
    }

    private sealed class Item { public int Id { get; set; } public string? Name { get; set; } }

    private static TestDb CreateDb()
    {
        var opts = new DbContextOptionsBuilder<TestDb>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDb(opts);
    }

    [Fact]
    public async Task Commit_Persists()
    {
        await using var db = CreateDb();
        var uow = new EfUnitOfWork(db);

        await using (var tx = await uow.BeginAsync())
        {
            db.Items.Add(new Item { Name = "ok" });
            await uow.SaveChangesAsync();
            await tx.CommitAsync();
        }

        Assert.Single(await db.Items.ToListAsync());
    }

    [Fact]
    public async Task Dispose_WithoutCommit_RollsBack()
    {
        await using var db = CreateDb();
        var uow = new EfUnitOfWork(db);

        await using (var tx = await uow.BeginAsync())
        {
            db.Items.Add(new Item { Name = "nope" });
            await uow.SaveChangesAsync();
            // без CommitAsync
        }

        Assert.Empty(await db.Items.ToListAsync());
    }

    [Fact]
    public async Task Nested_Savepoints_Work()
    {
        await using var db = CreateDb();
        var uow = new EfUnitOfWork(db);

        await using (var root = await uow.BeginAsync())
        {
            db.Items.Add(new Item { Name = "root" });
            await uow.SaveChangesAsync();

            await using (var nested = await uow.BeginAsync())
            {
                db.Items.Add(new Item { Name = "nested" });
                await uow.SaveChangesAsync();
                // откатываем вложенную
                // not calling Commit on nested
            }

            await root.CommitAsync();
        }

        var items = await db.Items.ToListAsync();
        Assert.Single(items);
        Assert.Equal("root", items[0].Name);
    }
}
```

---

# Замечания по использованию

* Все репозитории/хэндлеры должны получать **один и тот же scoped `AppDbContext`** через DI — тогда они автоматически участвуют в текущей транзакции. `AddDbContextPool` это не ломает.
* Не создавай новый `DbContext` внутри репозитория (и не открывай новый scope), если хочешь остаться в транзакции.
* Для Postgres блокировки строк делай через:

  ```csharp
  .FromSqlInterpolated($"SELECT * FROM table WHERE id = {id} FOR NO KEY UPDATE")
  ```

  внутри `Begin/Commit`.
* Где важны ретраи транзиентных ошибок — используй `ExecuteWithRetryAsync`.

Если хочешь, могу собрать это в маленький NuGet-пакет (namespace переименуем под твой проект) и добавить README.
