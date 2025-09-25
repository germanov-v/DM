Окей, без боксов и без плясок с Id. Раз тебе от `ChangeTracker` нужно только «съесть сущности и разослать их доменные события», делаем так:

* `ChangeTracker` принимает **любой** `IEntity` — ему не важно, что за Aggregate и какой у него Id.
* События остаются типобезопасными: каждое событие — свой класс.
* Диспетчеризация делается по **фактическим типам**: `(тип агрегата, тип события) → зарегистрированные хендлеры`.
* Никаких `object`-Id и боксов; ключи — `RuntimeTypeHandle` (значимые типы, без аллокаций).
* Снаружи ты не «кастишь событие», а просто регистрируешь обработчик под его тип.

### Контракты (минимум)

```csharp
public interface IEntity
{
    // ВАЖНО: только чтение. Внешний код не должен сеттить события.
    IReadOnlyCollection<IDomainEvent> Events { get; }
    void ClearDomainEvents();
}

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}

// Маркер «событие для TAggregate»
public interface IDomainEvent<TAggregate> : IDomainEvent where TAggregate : IEntity { }

// Типобезопасный обработчик
public interface IDomainEventHandler<in TAggregate, in TEvent>
    where TAggregate : IEntity
    where TEvent : IDomainEvent<TAggregate>
{
    ValueTask HandleAsync(TAggregate aggregate, TEvent @event, CancellationToken ct);
}
```

### Пример событий/агрегатов

```csharp
public sealed class ProductCreated : IDomainEvent<Product>
{
    public ProductCreated(Guid productId, string name) 
        => (ProductId, Name, OccurredOn) = (productId, name, DateTime.UtcNow);

    public Guid ProductId { get; }
    public string Name { get; }
    public DateTime OccurredOn { get; }
}

public sealed class Product : IEntity
{
    private readonly List<IDomainEvent> _events = new();

    public IReadOnlyCollection<IDomainEvent> Events => _events;
    public void ClearDomainEvents() => _events.Clear();

    public void Create(Guid id, string name)
    {
        // ... доменная логика
        _events.Add(new ProductCreated(id, name));
    }
}
```

### Реестр обработчиков событий (без бокса, без отражения в рантайме)

Регистрируем обработчики один раз — получаем делегаты-обёртки, которые вызывают типобезопасный хендлер без рефлексии на публикации.

```csharp
using System.Collections.Concurrent;

public sealed class DomainEventBus
{
    // Ключ по типу агрегата и типу события — без аллокаций и бокса
    private readonly struct Key : IEquatable<Key>
    {
        public readonly RuntimeTypeHandle Aggregate;
        public readonly RuntimeTypeHandle Event;

        public Key(Type aggregate, Type @event)
        {
            Aggregate = aggregate.TypeHandle;
            Event = @event.TypeHandle;
        }

        public bool Equals(Key other) => Aggregate.Equals(other.Aggregate) && Event.Equals(other.Event);
        public override int GetHashCode() => HashCode.Combine(Aggregate, Event);
    }

    // Делегат-обёртка одного вызова
    private delegate ValueTask EventInvoker(IEntity aggregate, IDomainEvent @event, CancellationToken ct);

    private readonly ConcurrentDictionary<Key, ImmutableArray<EventInvoker>> _map = new();

    public void Subscribe<TAgg, TEvent>(IDomainEventHandler<TAgg, TEvent> handler)
        where TAgg : class, IEntity
        where TEvent : class, IDomainEvent<TAgg>
    {
        // Создаём замыкание без рефлексии: компилятор сам кастует
        EventInvoker inv = static async (agg, ev, ct) =>
        {
            await handler.HandleAsync((TAgg)agg, (TEvent)ev, ct);
        };

        var key = new Key(typeof(TAgg), typeof(TEvent));

        _map.AddOrUpdate(
            key,
            _ => ImmutableArray.Create(inv),
            (_, arr) => arr.Add(inv));
    }

    public async ValueTask PublishAsync(IEntity aggregate, IDomainEvent @event, CancellationToken ct = default)
    {
        var key = new Key(aggregate.GetType(), @event.GetType());
        if (_map.TryGetValue(key, out var invokers))
        {
            foreach (var inv in invokers)
                await inv(aggregate, @event, ct);
        }
    }
}
```

### ChangeTracker: только собирает сущности и «сливает» их события в шину

Никаких Id, только ссылки на сущности. Для предотвращения утечек — `WeakReference`.

```csharp
using System.Collections.Concurrent;

public sealed class ChangeTracker
{
    private readonly ConcurrentDictionary<int, WeakReference<IEntity>> _entities = new();

    public void Track(IEntity entity)
    {
        // Ключ — хеш по ссылке (без аллокаций)
        var key = RuntimeHelpers.GetHashCode(entity);
        _entities[key] = new WeakReference<IEntity>(entity);
    }

    public async ValueTask FlushAsync(DomainEventBus bus, CancellationToken ct = default)
    {
        foreach (var kv in _entities.ToArray())
        {
            if (!kv.Value.TryGetTarget(out var entity))
            {
                _entities.TryRemove(kv.Key, out _);
                continue;
            }

            // Берём снимок списка событий, чтобы обработчики могли добавлять новые
            var events = entity.Events.ToArray();
            if (events.Length == 0) continue;

            foreach (var ev in events)
                await bus.PublishAsync(entity, ev, ct);

            entity.ClearDomainEvents();
        }
    }
}
```

### Пример использования

```csharp
public sealed class ProductCreatedHandler : IDomainEventHandler<Product, ProductCreated>
{
    public ValueTask HandleAsync(Product aggregate, ProductCreated @event, CancellationToken ct)
    {
        // тут у тебя уже «конкретный» тип события, без кастов/рефлексии
        Console.WriteLine($"Product created: {@event.ProductId}, {@event.Name}");
        return ValueTask.CompletedTask;
    }
}

// -- где-то в composition root:

var bus = new DomainEventBus();
bus.Subscribe(new ProductCreatedHandler());

var tracker = new ChangeTracker();

var product = new Product();
product.Create(Guid.NewGuid(), "Coffee Mug");
tracker.Track(product);

// ...
await tracker.FlushAsync(bus);
```

### Почему это решает твою боль

* **Не нужен каст снаружи.** Тип события известен там, где ты его обрабатываешь — через generic‑хендлер. Вызов туда идёт по `(AggregateType, EventType)`, что гарантирует корректный тип.
* **Без боксов.** Мы не тащим `object Id`. Ключи — `RuntimeTypeHandle` (значимые типы, не аллоцируют). Сами события/агрегаты — ссылки.
* **Без рефлексии при публикации.** Всё «сшивается» через закрытые generic‑делегаты, созданные при `Subscribe`.
* **Чистые агрегаты.** `IEntity` отдает `IReadOnlyCollection<IDomainEvent>` + `ClearDomainEvents()`. Никто, кроме агрегата, не пушит туда события.

Если захочешь ещё жёстче типизировать, можно добавить маркер в событие:

```csharp
public interface IDomainEvent<out TAggregate, out TSelf>
    : IDomainEvent<TAggregate>
    where TAggregate : IEntity
    where TSelf : IDomainEvent<TAggregate> { }
```

и маппить по `typeof(TSelf)`; но на практике пара `(AggregateType, EventType)` уже даёт отличную развязку.



Еще правки:

```csharp
public sealed class ChangeTracker
{
    private readonly ConcurrentDictionary<IEntity, WeakReference<IEntity>> _entities =
        new(ReferenceEqualityComparer.Instance);

    public void Track(IEntity entity)
        => _entities.TryAdd(entity, new WeakReference<IEntity>(entity));

    public async ValueTask FlushAsync(DomainEventBus bus, CancellationToken ct = default)
    {
        foreach (var (key, weak) in _entities.ToArray())
        {
            if (!weak.TryGetTarget(out var entity))
            {
                _entities.TryRemove(key, out _);
                continue;
            }

            // надёжно «вычерпываем» все события, включая добавленные обработчиками
            while (true)
            {
                var batch = entity.DequeueDomainEvents(); // ← сделай такой метод у агрегата
                if (batch.Count == 0) break;

                foreach (var ev in batch)
                    await bus.PublishAsync(entity, ev, ct).ConfigureAwait(false);
            }
        }
    }
}
```