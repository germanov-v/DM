
Ошибка на static, но меньше аллокаций:

```csharp
   public void Subscribe<TEntity, TEvent>(IEventHandler<TEntity, TEvent> handler)
        where TEntity : class, IEntity
        where TEvent : class, IEvent<TEntity>
    {
        EventInvoker invoker =  static async (entity, @event, cancellationToken) =>
        {
            await handler.Handle((TEntity)entity, (TEvent)@event, cancellationToken);
        };
        
        var key = new KeyEvent(typeof(TEntity), typeof(TEvent));
        
        _handlers.AddOrUpdate(key,
            _=> ImmutableArray.Create<EventInvoker>(invoker),
            (_, arr) => arr.Add(invoker)
            );
    }
```


Решение:

Да, это ожидаемо: `static`‑лямбда **не может ссылаться на внешние переменные**, а `handler` — как раз внешний параметр метода `Subscribe`. Есть три нормальных пути — от «просто работает» до «микро‑оптимально без аллокаций closure».

---

## Вариант A (проще всего): убираем `static`

Если подписки редкие → лишняя аллокация closure некритична.

```csharp
EventInvoker invoker = async (entity, @event, ct) =>
{
    await handler.HandleAsync((TEntity)entity, (TEvent)@event, ct);
};
```

(Можно убрать `async` и просто вернуть `handler.HandleAsync(...)`, чтобы не плодить state machine.)

---

## Вариант B (перфоманс, без closure): «state + static»

Добавляем «состояние» в элемент таблицы обработчиков и держим `handler` в нём, а лямбда остаётся `static`.

```csharp
// 1) меняем тип, который храним в словаре
private readonly record struct Entry(EventInvoker Invoker, object State);
private readonly ConcurrentDictionary<Key, ImmutableArray<Entry>> _map = new();

// 2) делегат принимает state
private delegate ValueTask EventInvoker(object state, IEntity aggregate, IDomainEvent @event, CancellationToken ct);

// 3) подписка без захвата
public void Subscribe<TEntity, TEvent>(IEventHandler<TEntity, TEvent> handler)
    where TEntity : class, IEntity
    where TEvent   : class, IEvent<TEntity>
{
    var key = new Key(typeof(TEntity), typeof(TEvent));

    // handler передаём через state, лямбда static и ничего не захватывает
    var entry = new Entry(
        invoker: static (state, agg, ev, ct) =>
            ((IEventHandler<TEntity, TEvent>)state).HandleAsync((TEntity)agg, (TEvent)ev, ct),
        state: handler);

    _map.AddOrUpdate(
        key,
        _ => ImmutableArray.Create(entry),
        (_, arr) => arr.Add(entry));
}

// 4) публикация
public async ValueTask PublishAsync(IEntity aggregate, IDomainEvent @event, CancellationToken ct = default)
{
    var key = new Key(aggregate.GetType(), @event.GetType());
    if (_map.TryGetValue(key, out var entries))
    {
        // последовательно или собери tasks и дождись всех
        foreach (var e in entries)
            await e.Invoker(e.State, aggregate, @event, ct).ConfigureAwait(false);
    }
}
```

Плюсы:

* нет closure, `static` лямбда валидна;
* только хранение пары `(Invoker, State)`;
* публикация без аллокаций.

---

## Вариант C (без лямбд): адаптер‑класс

Создать маленький объект‑адаптер с методом нужной сигнатуры; делегат — method group без closure.

```csharp
private sealed class BoundInvoker<TEntity, TEvent>
    where TEntity : class, IEntity
    where TEvent   : class, IEvent<TEntity>
{
    private readonly IEventHandler<TEntity, TEvent> _handler;
    public BoundInvoker(IEventHandler<TEntity, TEvent> handler) => _handler = handler;

    public ValueTask Invoke(IEntity a, IDomainEvent e, CancellationToken ct) =>
        _handler.HandleAsync((TEntity)a, (TEvent)e, ct);
}

// подписка:
var bound = new BoundInvoker<TEntity, TEvent>(handler);
EventInvoker invoker = bound.Invoke; // method group, без closure
```

Плюс: просто и понятно; минус: одна маленькая аллокация адаптера на подписку.

---

### Что выбрать

* Нужен максимум производительности и «чистый» publish → **Вариант B**.
* Хочешь проще → **Вариант A**.
* Любишь method group и без лямбд → **Вариант C**.

Дополнительно: в `PublishAsync` лучше убрать `async` в лямбдах и не создавать лишние state machines — возвращай `ValueTask` напрямую и (если нужно параллелить) набирай массив `ValueTask` и жди каждого в цикле, собирая исключения.
