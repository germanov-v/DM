Да, ты правильно видишь проблему: в твоей версии `Subscribe` хранит **экземпляры** хэндлеров. Если `LocalEventBus` — `Singleton`, то хэндлеры “законсервируются” тоже как синглтоны → убьёшь `Scoped`‑зависимости (логгер/репозитории и т.п.). Если сделать шину `Scoped` — придётся **каждый scope заново подписывать** всех обработчиков → шум и оверхед.

Решение: **шина не хранит экземпляры**, а хранит **типы обработчиков и подготовленный инвокер**, а сами обработчики в момент публикации достаёт из **DI‑scope**. Тогда:

* `LocalEventBus` = `Singleton`.
* Обработчики = `Scoped` (или `Transient`) — всё ОК.
* Никакого дублирования подписок между scope’ами.

Вот минимально‑боевой вариант.

---

### Идея

* В реестре держим: ключ `(TEntity, TEvent)` → **тип сервиса** `IEventHandler<TEntity,TEvent>` + **скомпилированный делегат** вызова `Handle`.
* На `Publish` создаём `IServiceScope`, делаем `GetServices(closedHandlerType)`, прогоняем всех.

### Код

```csharp
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;

public sealed class LocalEventBus
{
    private readonly IServiceScopeFactory _scopeFactory;
    public LocalEventBus(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

    private readonly struct Key : IEquatable<Key>
    {
        public readonly Type EntityType;
        public readonly Type EventType;
        public Key(Type e, Type ev) { EntityType = e; EventType = ev; }
        public bool Equals(Key o) => EntityType == o.EntityType && EventType == o.EventType;
        public override int GetHashCode() => HashCode.Combine(EntityType, EventType);
    }

    // (handlerType, invoker)
    private readonly ConcurrentDictionary<Key,(Type handlerType, HandlerInvoker invoker)> _cache = new();

    private delegate ValueTask HandlerInvoker(object handler, object entity, object @event, CancellationToken ct);

    // Регистрация типов (обычно на старте)
    public void Register<TEntity, TEvent>()
        where TEntity : class, IEntity
        where TEvent   : class, IEvent
    {
        var key = new Key(typeof(TEntity), typeof(TEvent));
        _cache.GetOrAdd(key, static k =>
        {
            var closed = typeof(IEventHandler<,>).MakeGenericType(k.EntityType, k.EventType);
            var inv    = BuildInvoker(closed, k.EntityType, k.EventType);
            return (closed, inv);
        });
    }

    public async ValueTask PublishAsync(IEntity entity, IEvent @event, CancellationToken ct = default)
    {
        var key = new Key(entity.GetType(), @event.GetType());
        if (!_cache.TryGetValue(key, out var entry)) return;

        using var scope = _scopeFactory.CreateScope();
        var handlers = scope.ServiceProvider.GetServices(entry.handlerType);
        if (handlers is null) return;

        foreach (var h in handlers)
        {
            var vt = entry.invoker(h!, entity, @event, ct);
            if (!vt.IsCompletedSuccessfully) await vt.ConfigureAwait(false);
        }
    }

    private static HandlerInvoker BuildInvoker(Type closedHandler, Type entityType, Type eventType)
    {
        // ищем Handle(TEntity,TEvent, CancellationToken)
        var mi = closedHandler.GetMethod(nameof(IEventHandler<IEntity, IEvent>.Handle))!;

        var h  = Expression.Parameter(typeof(object), "h");
        var e  = Expression.Parameter(typeof(object), "e");
        var ev = Expression.Parameter(typeof(object), "ev");
        var ct = Expression.Parameter(typeof(CancellationToken), "ct");

        var call = Expression.Call(
            Expression.Convert(h, closedHandler),
            mi,
            Expression.Convert(e,  entityType),
            Expression.Convert(ev, eventType),
            ct);

        return Expression.Lambda<HandlerInvoker>(call, h, e, ev, ct).Compile();
    }
}
```

#### Регистрация DI

```csharp
// Обработчики — Scoped/Transient
services.AddScoped<IEventHandler<User, UserRegisterByEmail>, UserRegisterByEmailHandler>();

// Шина — Singleton
services.AddSingleton<LocalEventBus>();

// На старте (Program.cs) «заводи» пары типов:
app.Services.GetRequiredService<LocalEventBus>()
    .Register<User, UserRegisterByEmail>();
```

> Если не хочешь вручную вызывать `Register<,>()`, можно один раз просканировать контейнер и вызвать для всех зарегистрированных `IEventHandler<,>`.

---

### Почему это решает твою проблему

* Шина **не держит инстансы** → нет утечки lifetime’а.
* Обработчики создаются **из актуального DI‑scope** → логгер, репозитории и прочее — корректные `Scoped`.
* **Нет рефлексии на горячем пути**: рефлексия/Expression‑Compile один раз при `Register`, дальше — быстрый делегат.
* Никаких “подписок” per scope — `Register` вызывается один раз при старте.

Если не хочется вообще делать свои `Register`, можно полностью отказаться от реестра и на `Publish` строить `(closedType, invoker)` через кэш `GetOrAdd` по реальным типам — тоже ок, просто первый вызов каждой пары типов будет чуть дороже.
