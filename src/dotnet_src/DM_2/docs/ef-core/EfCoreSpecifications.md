Коротко: для «универсализации» запросов в EF Core чаще всего используют **Specification** (с «оценщиком»/Evaluator, который превращает спецификацию в `IQueryable`), плюс **Query Object** для сложных read-моделей. Ниже — минимальные, но боевые шаблоны с примерами.

# 1) Базовый каркас Specification

```csharp
public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    List<string> IncludeStrings { get; }              // для ThenInclude цепочек строками (опционально)
    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }
    int? Skip { get; }
    int? Take { get; }
    bool AsNoTracking { get; }
    bool AsSplitQuery { get; }
}

public abstract class Specification<T> : ISpecification<T>
{
    public Expression<Func<T, bool>>? Criteria { get; protected set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<T, object>>? OrderBy { get; protected set; }
    public Expression<Func<T, object>>? OrderByDescending { get; protected set; }
    public int? Skip { get; protected set; }
    public int? Take { get; protected set; }
    public bool AsNoTracking { get; protected set; } = true;
    public bool AsSplitQuery { get; protected set; } = false;

    protected void AddInclude(Expression<Func<T, object>> include) => Includes.Add(include);
    protected void AddInclude(string include) => IncludeStrings.Add(include);
    protected void ApplyOrderBy(Expression<Func<T, object>> orderBy) => OrderBy = orderBy;
    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDesc) => OrderByDescending = orderByDesc;
    protected void ApplyPaging(int skip, int take) { Skip = skip; Take = take; }
    protected void UseSplitQuery() => AsSplitQuery = true;
    protected void TrackChanges() => AsNoTracking = false;
}
```

**Оценщик (Evaluator)**:

```csharp
public static class SpecificationEvaluator
{
    public static IQueryable<T> GetQuery<T>(IQueryable<T> inputQuery, ISpecification<T> spec)
        where T : class
    {
        var query = inputQuery;

        if (spec.Criteria is not null)
            query = query.Where(spec.Criteria);

        if (spec.OrderBy is not null)
            query = query.OrderBy(spec.OrderBy);
        else if (spec.OrderByDescending is not null)
            query = query.OrderByDescending(spec.OrderByDescending);

        foreach (var include in spec.Includes)
            query = query.Include(include);

        foreach (var inc in spec.IncludeStrings)
            query = query.Include(inc);

        if (spec.AsSplitQuery)
            query = query.AsSplitQuery();

        if (spec.AsNoTracking)
            query = query.AsNoTracking();

        if (spec.Skip.HasValue) query = query.Skip(spec.Skip.Value);
        if (spec.Take.HasValue) query = query.Take(spec.Take.Value);

        return query;
    }
}
```

**Репозиторий с поддержкой спецификаций**:

```csharp
public interface IRepository<T> where T: class
{
    Task<T?> GetByIdAsync(object id, CancellationToken ct);
    Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken ct);
    Task<List<T>> ListAsync(ISpecification<T> spec, CancellationToken ct);
    Task<int> CountAsync(ISpecification<T> spec, CancellationToken ct);
    Task AddAsync(T entity, CancellationToken ct);
    // ...
}

public sealed class EfRepository<T> : IRepository<T> where T : class
{
    private readonly DbContext _db;
    public EfRepository(DbContext db) => _db = db;

    public Task<T?> GetByIdAsync(object id, CancellationToken ct) =>
        _db.Set<T>().FindAsync(new[] { id }, ct).AsTask();

    public Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken ct) =>
        SpecificationEvaluator.GetQuery(_db.Set<T>().AsQueryable(), spec).FirstOrDefaultAsync(ct);

    public Task<List<T>> ListAsync(ISpecification<T> spec, CancellationToken ct) =>
        SpecificationEvaluator.GetQuery(_db.Set<T>().AsQueryable(), spec).ToListAsync(ct);

    public Task<int> CountAsync(ISpecification<T> spec, CancellationToken ct) =>
        SpecificationEvaluator.GetQuery(_db.Set<T>().AsQueryable(), spec).CountAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct)
    {
        await _db.Set<T>().AddAsync(entity, ct);
    }
}
```

# 2) Примеры спецификаций

### Заказы клиента за период, с линиями, по убыванию даты, постранично

```csharp
public sealed class OrdersByCustomerInPeriodSpec : Specification<Order>
{
    public OrdersByCustomerInPeriodSpec(CustomerId customerId, DateOnly from, DateOnly to, int page, int size)
    {
        Criteria = o => o.CustomerId == customerId 
                        && o.CreatedAt >= from.ToDateTime(TimeOnly.MinValue)
                        && o.CreatedAt <  to.ToDateTime(TimeOnly.MaxValue);

        AddInclude(o => o.Lines);             // внутренняя часть агрегата
        ApplyOrderByDescending(o => o.CreatedAt);
        ApplyPaging((page - 1) * size, size);
        UseSplitQuery();                      // если много коллекций — поможет
    }
}
```

### Поиск товаров по строке с сортировкой

```csharp
public sealed class ProductsSearchSpec : Specification<Product>
{
    public ProductsSearchSpec(string? query, string? sort)
    {
        if (!string.IsNullOrWhiteSpace(query))
            Criteria = p => EF.Functions.ILike(p.Name, $"%{query.Trim()}%");

        ApplyOrderBy(sort switch
        {
            "name" => p => p.Name,
            "price" => p => p.Price,
            _ => p => p.Id
        });
    }
}
```

### Только подсчёт (без Includes)

```csharp
public sealed class ActiveUsersCountSpec : Specification<User>
{
    public ActiveUsersCountSpec()
    {
        Criteria = u => u.IsActive;
        // ни Includes, ни пагинации
    }
}
```

# 3) Проекции (Specification с результатом)

Полезно иметь вариант спецификации, которая **сразу проектирует** в DTO. Это меньше тянет данных и снимает соблазн `Include`.

```csharp
public interface IProjectingSpecification<T, TResult> : ISpecification<T>
{
    Expression<Func<T, TResult>> Selector { get; }
}

public abstract class ProjectingSpecification<T, TResult>
    : Specification<T>, IProjectingSpecification<T, TResult>
{
    public required Expression<Func<T, TResult>> Selector { get; init; }
}
```

**Evaluator для проекций:**

```csharp
public static class ProjectionEvaluator
{
    public static IQueryable<TResult> GetQuery<T, TResult>(
        IQueryable<T> input, IProjectingSpecification<T, TResult> spec)
        where T : class
    {
        var q = SpecificationEvaluator.GetQuery(input, spec);
        return q.Select(spec.Selector);
    }
}
```

**Репозиторий-ридер для DTO:**

```csharp
public interface IReadRepository<T> where T: class
{
    Task<List<TResult>> ListAsync<TResult>(IProjectingSpecification<T, TResult> spec, CancellationToken ct);
    Task<TResult?> FirstOrDefaultAsync<TResult>(IProjectingSpecification<T, TResult> spec, CancellationToken ct);
}

public sealed class EfReadRepository<T> : IReadRepository<T> where T : class
{
    private readonly DbContext _db;
    public EfReadRepository(DbContext db) => _db = db;

    public Task<List<TResult>> ListAsync<TResult>(IProjectingSpecification<T, TResult> spec, CancellationToken ct) =>
        ProjectionEvaluator.GetQuery(_db.Set<T>(), spec).ToListAsync(ct);

    public Task<TResult?> FirstOrDefaultAsync<TResult>(IProjectingSpecification<T, TResult> spec, CancellationToken ct) =>
        ProjectionEvaluator.GetQuery(_db.Set<T>(), spec).FirstOrDefaultAsync(ct);
}
```

**Пример проекции «Список заказов» без `Include`:**

```csharp
public sealed class OrderListItemDto
{
    public Guid Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public decimal Total { get; init; }
    public string CustomerName { get; init; } = default!;
}

public sealed class OrdersListSpec : ProjectingSpecification<Order, OrderListItemDto>
{
    public OrdersListSpec(int page, int size)
    {
        ApplyOrderByDescending(o => o.CreatedAt);
        ApplyPaging((page - 1) * size, size);

        Selector = o => new OrderListItemDto
        {
            Id = o.Id,
            CreatedAt = o.CreatedAt,
            Total = o.Lines.Sum(l => l.Price * l.Qty),
            CustomerName = o.Customer.Name // если это та же таблица/вью, иначе подтянут join
        };
    }
}
```

# 4) Динамическая фильтрация/сортировка (простые утилиты)

```csharp
public static class QueryablePagingExtensions
{
    public static IQueryable<T> Page<T>(this IQueryable<T> q, int page, int size)
        => q.Skip((page - 1) * size).Take(size);
}

public static class QueryableSortExtensions
{
    public static IQueryable<T> OrderBy<T>(this IQueryable<T> q, string propertyName, bool desc = false)
    {
        var param = Expression.Parameter(typeof(T), "x");
        var body = propertyName.Split('.')
            .Aggregate((Expression)param, Expression.PropertyOrField);
        var lambda = Expression.Lambda(body, param);

        var method = desc ? "OrderByDescending" : "OrderBy";
        var call = Expression.Call(typeof(Queryable), method,
            new[] { typeof(T), body.Type }, q.Expression, Expression.Quote(lambda));
        return q.Provider.CreateQuery<T>(call);
    }
}
```

# 5) Компилированные запросы EF Core (горячие пути)

Для частых запросов с одинаковой формой:

```csharp
public static class CompiledQueries
{
    public static readonly Func<AppDbContext, Guid, CancellationToken, Task<Order?>> GetOrderWithLines =
        EF.CompileAsyncQuery((AppDbContext db, Guid id, CancellationToken ct) =>
            db.Orders
              .Include(o => o.Lines)
              .AsNoTracking()
              .FirstOrDefault(o => o.Id == id));
}
```

Использование:

```csharp
var order = await CompiledQueries.GetOrderWithLines(_db, orderId, ct);
```

# 6) Где что применять (правило выбора)

* **Мелкие повторяемые фильтры/срезы** → Specification (переиспользуемая логика LINQ, одинаково работает в репозитории).
* **Сложные отчёты/витрины/DTO** → ProjectingSpecification или отдельный **Query Object**/хендлер с `Select(...)` и явными `join`.
* **Горячие маршруты** → Компилированные запросы.
* **Большие графы** → избегаем `Include` вне агрегата; делаем проекции. Если несколько коллекций — `AsSplitQuery()`.

Если хочешь, могу «прикрутить» это к твоим сущностям `User`, `Role`, `Order` и показать 2–3 готовых спеки и их использование в твоём репозитории/хендлерах.
