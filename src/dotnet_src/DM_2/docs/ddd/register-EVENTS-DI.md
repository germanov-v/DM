Коротко: проверка `handlerInterfaceType.IsAssignableFrom(type)` **не работает для open generic**.
`IsAssignableFrom` сравнивает *конструированные* типы (закрытые). Когда слева — `typeof(IEventHandler<,>)`, а справа — `SomeHandler<,>` или даже `SomeHandler<TUser, TEvent>`, результат будет `false`. Поэтому такие классы ты отфильтруешь и не зарегистрируешь.

Нужно **не проверять IsAssignableFrom**, а явно искать:

1. реализует ли класс **open generic** интерфейс `IEventHandler<,>` (для регистрации open generic);
2. какие **closed generic** интерфейсы `IEventHandler<TEntity,TEvent>` он реализует (чтобы добавить конкретные сервис-дескрипторы и заполнить реестр).

Вот рабочая реализация без лишней фильтрации — с комментариями:

```csharp
public static class EventHandlersDiExtensions
{
    public static IServiceCollection AddEventHandlers(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        var openIface = typeof(IEventHandler<,>);
        var types = assemblies.SelectMany(SafeGetTypes)
                              .Where(t => t is { IsClass: true, IsAbstract: false });

        // key -> список интерфейсов (а не один!)
        var map = new Dictionary<KeyEvent, List<Type>>();

        foreach (var type in types)
        {
            // 1) Если это open-generic реализация IEventHandler<,>
            //    (например, class AuditHandler<TEntity,TEvent> : IEventHandler<TEntity,TEvent>)
            if (type.IsGenericTypeDefinition && ImplementsOpenGeneric(type, openIface))
            {
                // Регистрируем open generic: IEventHandler<,> -> AuditHandler<,>
                // Тут как раз TryAddEnumerable ОК.
                services.TryAddEnumerable(ServiceDescriptor.Transient(openIface, type));
                // В реестр такие не кладём — они закрываются позднее по фактической паре типов.
                continue;
            }

            // 2) Все закрытые интерфейсы IEventHandler<TEntity,TEvent>, которые реализует класс
            var closedIfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openIface)
                .ToArray();

            if (closedIfaces.Length == 0)
                continue;

            foreach (var ci in closedIfaces)
            {
                // Регистрируем конкретную реализацию
                services.TryAddEnumerable(ServiceDescriptor.Transient(ci, type));

                // Наполняем реестр отображением (TEntity,TEvent) -> интерфейс
                var key = GetKeyEvent(ci);
                if (!map.TryGetValue(key, out var list))
                    map[key] = list = new List<Type>();
                list.Add(ci);
            }
        }

        services.AddSingleton(new HandlerRegistry(map));
        services.AddScoped<HandlerProvider>();
        return services;
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly a)
    {
        try { return a.GetTypes(); }
        catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null)!; }
    }

    private static bool ImplementsOpenGeneric(Type type, Type openGenericInterface)
    {
        // проверяем и цепочку базовых типов (на случай, если реализация унаследована)
        return EnumerateTypeHierarchy(type)
            .SelectMany(t => t.GetInterfaces())
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericInterface);
    }

    private static IEnumerable<Type> EnumerateTypeHierarchy(Type t)
    {
        for (var cur = t; cur != null && cur != typeof(object); cur = cur.BaseType!)
            yield return cur;
    }

    public static KeyEvent GetKeyEvent(Type iface)
    {
        var args = iface.GetGenericArguments(); // [TEntity, TEvent]
        return new KeyEvent(args[0], args[1]);
    }
}
```

### Что изменилось и почему это правильно

* **Убрали `IsAssignableFrom`**: он бесполезен для open generic кейса и скрывает валидные типы.
* **Явно ищем open generic** через `ImplementsOpenGeneric`: смотрим `GetInterfaces()` и `GetGenericTypeDefinition() == typeof(IEventHandler<,>)`.
* **Регистрируем open generic** напрямую: `TryAddEnumerable(Transient(typeof(IEventHandler<,>), type))`.
* **Закрытые интерфейсы** достаём отдельно и регистрируем каждый; **в реестр** складываем **список** интерфейсов на один `KeyEvent`.

Так твои open generic обработчики будут корректно подхватываться DI как open generic, а закрытые — как конкретные реализации; и диспетчеризация по `(TEntity, TEvent)` станет надёжной.
