```csharp
public interface IDomainEventHandler<in T> where T : IDomainEvent
{
    Task Handle(T @event, CancellationToken ct);
}

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct);
}

public sealed class DomainEventDispatcher(IServiceProvider sp) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct)
    {
        foreach (var e in events)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(e.GetType());
            var handlers = (IEnumerable<object>)sp.GetServices(handlerType);

            foreach (var h in handlers)
            {
                var method = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.Handle))!;
                var task = (Task)method.Invoke(h, new object[] { e, ct })!;
                await task.ConfigureAwait(false);
            }
        }
    }
}

public sealed class AppDbContext : DbContext
{
    private readonly IDomainEventDispatcher _dispatcher;
    public AppDbContext(DbContextOptions o, IDomainEventDispatcher d) : base(o) => _dispatcher = d;

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // 1) собрать события ДО сохранения
        var events = ChangeTracker.Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .SelectMany(a => a.DequeueDomainEvents())   // снимаем и очищаем
            .ToList();

        // 2) сохранить данные
        var result = await base.SaveChangesAsync(ct);

        // 3) опубликовать ПОСЛЕ коммита
        await _dispatcher.DispatchAsync(events, ct);

        return result;
    }
}

```