Коротко: **в `CommitChanges` ничего “регистрировать” не надо**. Либо вообще **убери `LocalEventBus`** и шли события через DI/mediator, либо преврати шину в тонкий адаптер над `IServiceProvider`, который на публикации сам достаёт обработчики из DI.

Вот два нормальных варианта.

---

# Вариант A — вообще без своей шины (через DI/MediatR)

**Идея:** в домене есть только события. В `CommitChanges` ты после коммита просто проходишься по событиям и вызываешь соответствующие обработчики из DI. Самый простой способ — MediatR `IPublisher.Publish()`.

```csharp
// Domain
public interface IDomainEvent : INotification {}

public abstract class AggregateRoot {
    private readonly List<IDomainEvent> _events = new();
    protected void Raise(IDomainEvent e) => _events.Add(e);
    public IReadOnlyCollection<IDomainEvent> DequeueEvents() { var c = _events.ToArray(); _events.Clear(); return c; }
}

// CommitChanges (после успешного сохранения/коммита)
public sealed class UnitOfWork
{
    private readonly IPublisher _publisher;     // MediatR
    private readonly IChangeTracker _tracker;

    public async Task CommitChanges(CancellationToken ct = default)
    {
        // 1) commit БД…
        // 2) события после коммита
        foreach (var entity in _tracker.GetAllAndClear())
        {
            foreach (var ev in entity.DequeueDomainEvents())
                await _publisher.Publish(ev, ct);
        }
    }
}
```

Хэндлер:

```csharp
public sealed class UserRegisterByEmailHandler : INotificationHandler<UserRegisterByEmail>
{
    private readonly ILogger<UserRegisterByEmailHandler> _log;
    private readonly IEmailSender _email;

    public async Task Handle(UserRegisterByEmail e, CancellationToken ct)
    {
        _log.LogInformation("User {Id} registered by {Email}", e.UserId, e.Email);
        await _email.SendWelcomeAsync(e.Email, ct);
    }
}
```

**Плюсы:** не дублируешь функционал DI; легко тянуть `ILogger` и любые сервисы; пайплайны MediatR, ретраи и т.п.
**Минусы:** зависимости от MediatR (не критично, можно и без него — см. Вариант B).

---

# Вариант B — тонкий `LocalEventBus` как адаптер над DI (без подписок)

**Идея:** не хранить никаких “Subscribe”. На публикации мы знаем `(тип агрегата, тип события)` — значит, можем **в момент публикации** достать из DI все `IEventHandler<TEntity,TEvent>` и вызвать их. Никаких замыканий и таблиц делегатов не нужно.

```csharp
public interface IEvent {}
public interface IEntity { /* … */ }

public interface IEventHandler<in TEntity, in TEvent>
    where TEntity : class, IEntity
    where TEvent   : class, IEvent
{
    ValueTask Handle(TEntity entity, TEvent @event, CancellationToken ct);
}

public sealed class LocalEventBus
{
    private readonly IServiceProvider _sp;

    public LocalEventBus(IServiceProvider sp) => _sp = sp;

    public async ValueTask PublishAsync(IEntity entity, IEvent @event, CancellationToken ct = default)
    {
        // Закрываем открытый generic по реальным типам
        var closedType = typeof(IEventHandler<,>).MakeGenericType(entity.GetType(), @event.GetType());

        // Достаём всех обработчиков этого закрытого generic’а
        var handlers = (IEnumerable<object>)_sp.GetServices(closedType);

        // Вызываем Handle у каждого
        foreach (var h in handlers)
        {
            var m = closedType.GetMethod(nameof(IEventHandler<IEntity, IEvent>.Handle))!;
            var task = (ValueTask)m.Invoke(h, new object[] { entity, @event, ct })!;
            await task.ConfigureAwait(false);
        }
    }
}
```

`CommitChanges`:

```csharp
public sealed class UnitOfWork
{
    private readonly IChangeTracker _tracker;
    private readonly LocalEventBus _bus;

    public async Task CommitChanges(CancellationToken ct = default)
    {
        // 1) commit БД…

        // 2) post-commit публикация
        foreach (var entity in _tracker.GetAllAndClear())
        {
            // вычерпываем все/новые события
            while (true)
            {
                var batch = entity.DequeueDomainEvents();
                if (batch.Count == 0) break;

                foreach (var ev in batch)
                    await _bus.PublishAsync(entity, ev, ct);
            }
        }
    }
}
```

**Плюсы:**

* Никаких собственных “реестров подписок”.
* Все зависимости (логгер, e‑mail и пр.) приходят через DI в конкретные хэндлеры.
* Минимум кода, нет дубля функционала DI.

**Оптимизация:** если не хочешь рефлексию каждый раз, можно один раз **закешировать** делегаты `EventInvoker` на `(TEntity,TEvent)` в `ConcurrentDictionary<TypePair, Invoker>` — это тонкая микрооптимизация.

---

## Ответы на твои вопросы

> как в CommitChanges получить зарегистрированные обработчики событий и их вызывать?

Не “получать зарегистрированные”. Делай **publish‑after‑commit**: достань у сущностей `DequeueDomainEvents()` и **через DI** вызови всех обработчиков `IEventHandler<TEntity,TEvent>` для конкретной пары типов (Вариант B), либо отдай событие в MediatR (Вариант A).

> Нет ли в LocalEventBus дубля функционала DI?

Если ты делаешь `Subscribe(...)` и хранишь свои массивы делегатов — **да, это дублирование**. В большинстве случаев он не нужен. Либо убери его (MediatR), либо пусть это будет **тонкий адаптер над `IServiceProvider`** без собственного реестра.

> но в обработчик события нужен логгер/сервисы

Именно поэтому обработчики должны жить в **Application/Infrastructure** и создаваться DI — тогда спокойно принимаешь `ILogger<T>`, репозитории, e‑mail и т.д.

---

## Что выбрать?

* Хочешь минимум своего кода → **MediatR `IPublisher.Publish`** в `CommitChanges` (самый популярный путь).
* Без MediatR → **тонкий `LocalEventBus` над DI** (см. Вариант B).

Оба подхода не ломают чистоту домена и решают твою задачу без лишней “самодельной шины”.
