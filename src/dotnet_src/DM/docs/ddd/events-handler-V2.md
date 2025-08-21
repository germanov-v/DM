```csharp
public interface IEvent { }
public interface ISubscriber<in TEvent> where TEvent : IEvent
{
    ValueTask HandleEventAsync(TEvent @event, CancellationToken ct = default);
}

using System.Collections.Concurrent;
using System.Collections.Immutable;

public interface IEvent { }

public interface ISubscriber<in TEvent> where TEvent : IEvent
{
    ValueTask HandleEventAsync(TEvent @event, CancellationToken ct = default);
}

public sealed class EventBroker
{
    // Ключ — тип события, значение — неизменяемый массив делегатов
    private readonly ConcurrentDictionary<Type, ImmutableArray<Func<IEvent, CancellationToken, ValueTask>>> _map
        = new();

    // Подписка через объект-подписчик
    public IDisposable Subscribe<TEvent>(ISubscriber<TEvent> subscriber) where TEvent : IEvent =>
        Subscribe<TEvent>((e, ct) => subscriber.HandleEventAsync(e, ct));

    // Подписка через делегат
    public IDisposable Subscribe<TEvent>(Func<TEvent, CancellationToken, ValueTask> handler) where TEvent : IEvent
    {
        var t = typeof(TEvent);
        Func<IEvent, CancellationToken, ValueTask> wrapper = (e, ct) => handler((TEvent)e, ct);

        _map.AddOrUpdate(t,
            _ => ImmutableArray.Create(wrapper),
            (_, arr) => arr.Add(wrapper));

        return new Unsubscriber(() =>
        {
            if (_map.TryGetValue(t, out var arr))
            {
                var updated = arr.Remove(wrapper);
                _map[t] = updated;
            }
        });
    }

    public async ValueTask<bool> PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : IEvent
    {
        if (!_map.TryGetValue(typeof(TEvent), out var handlers) || handlers.IsDefaultOrEmpty)
            return false;

        List<Exception>? errors = null;
        // Параллельный fan-out: каждый хэндлер выполняется независимо
        var tasks = new ValueTask[handlers.Length];
        for (int i = 0; i < handlers.Length; i++)
            tasks[i] = InvokeSafely(handlers[i], @event!, ct);

        // ждём всех и собираем ошибки
        for (int i = 0; i < tasks.Length; i++)
        {
            try { await tasks[i].ConfigureAwait(false); }
            catch (Exception ex)
            {
                (errors ??= new()).Add(ex);
            }
        }

        if (errors is { Count: > 0 })
            throw new AggregateException(errors);

        return true;

        static async ValueTask InvokeSafely(
            Func<IEvent, CancellationToken, ValueTask> h,
            IEvent e,
            CancellationToken c)
        {
            await h(e, c).ConfigureAwait(false);
        }
    }

    private sealed class Unsubscriber(Action dispose) : IDisposable
    {
        private Action? _dispose = dispose;
        public void Dispose() => Interlocked.Exchange(ref _dispose, null)?.Invoke();
    }
}

public sealed record UserRegistered(Guid UserId, string Email) : IEvent;

public sealed class SendWelcomeEmail : ISubscriber<UserRegistered>
{
    public ValueTask HandleEventAsync(UserRegistered e, CancellationToken ct = default)
    {
        Console.WriteLine($"Welcome, {e.Email}");
        return ValueTask.CompletedTask;
    }
}

var broker = new EventBroker();
using var sub = broker.Subscribe<UserRegistered>(new SendWelcomeEmail());

await broker.PublishAsync(new UserRegistered(Guid.NewGuid(), "user@site.com"));


public sealed class QueuedEventBroker : IAsyncDisposable
{
    private readonly EventBroker _inner = new();
    private readonly Channel<IEvent> _channel =
        Channel.CreateBounded<IEvent>(new BoundedChannelOptions(capacity: 10_000)
        { SingleReader = true, SingleWriter = false, FullMode = BoundedChannelFullMode.Wait });

    private readonly CancellationTokenSource _cts = new();
    private readonly Task _pump;

    public QueuedEventBroker() => _pump = Task.Run(PumpAsync);

    public IDisposable Subscribe<TEvent>(ISubscriber<TEvent> s) where TEvent : IEvent
        => _inner.Subscribe(s);

    public ValueTask EnqueueAsync(IEvent e, CancellationToken ct = default)
        => _channel.Writer.WriteAsync(e, ct);

    private async Task PumpAsync()
    {
        var r = _channel.Reader;
        while (await r.WaitToReadAsync(_cts.Token))
        {
            while (r.TryRead(out var e))
            {
                try
                {
                    var publishMethod = typeof(EventBroker)
                        .GetMethod(nameof(EventBroker.PublishAsync))!
                        .MakeGenericMethod(e.GetType());
                    await (ValueTask<bool>)publishMethod.Invoke(_inner, new object[] { e, _cts.Token })!;
                }
                catch (Exception ex)
                {
                    // лог/метрика/дед‑леттер по вкусу
                }
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _channel.Writer.TryComplete();
        try { await _pump; } catch { /* ignore */ }
        _cts.Dispose();
    }
}


```