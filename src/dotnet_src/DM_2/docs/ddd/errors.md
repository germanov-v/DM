# Заметки

Фиксированный ответ:

```json
{
  "type": "https://httpstatuses.com/404",
  "title": "Not Found",
  "status": 404,
  "detail": "The user with id '123' was not found.",
  "instance": "/api/users/123",
  "traceId": "00-9c1c40c48a774d9f9efb87cbb08a14d2-42b47e9f12c3a947-00"
}
```

Ответ валидации:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": [
      "'Email' is not a valid email address."
    ],
    "Password": [
      "'Password' must be at least 8 characters long."
    ]
  },
  "traceId": "00-1b2a3c4d5e6f7g8h9i0j..."
}
```


Для swagger:

```csharp
.Produces<AuthJwtResponse>(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status422UnprocessableEntity)
.ProducesProblem(StatusCodes.Status404NotFound)
.ProducesProblem(StatusCodes.Status409Conflict)
.ProducesProblem(StatusCodes.Status403Forbidden)
.ProducesProblem(StatusCodes.Status500InternalServerError);
```


FluentValidation Filter MinimalAPI:

```csharp
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

// 1) Регаем FV
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddProblemDetails(o =>
{
    o.CustomizeProblemDetails = ctx =>
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
});

// 2) Универсальный EndpointFilter для запроса типа TRequest
public sealed class ValidationFilter<TRequest> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        var validator = ctx.HttpContext.RequestServices.GetService<IValidator<TRequest>>();
        if (validator is null) return await next(ctx);

        // достаём аргумент запроса
        var arg = ctx.Arguments.OfType<TRequest>().FirstOrDefault();
        if (arg is null) return await next(ctx);

        ValidationResult vr = await validator.ValidateAsync(arg, ctx.HttpContext.RequestAborted);
        if (vr.IsValid) return await next(ctx);

        // строим каноничный ProblemDetails для 422
        var errors = vr.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        var http = ctx.HttpContext;
        var pd = new ProblemDetails
        {
            Status   = StatusCodes.Status422UnprocessableEntity,
            Title    = "Validation Error",
            Type     = "https://httpstatuses.com/422",
            Detail   = "One or more validation errors occurred.",
            Instance = http.Request.Path
        };
        pd.Extensions["errors"] = errors;
        pd.Extensions["traceId"] = http.TraceIdentifier;

        return TypedResults.Problem(pd);
    }
}

// 3) Удобный хелпер для маппинга эндпоинтов
public static class EndpointValidationExtensions
{
    public static RouteHandlerBuilder WithValidation<TReq>(this RouteHandlerBuilder builder)
        => builder.AddEndpointFilter(new ValidationFilter<TReq>());
}

// 4) Использование
app.MapPost("/register", (RegisterRequest req) =>
{
    // сюда попадём только если валидация прошла
    return Results.Ok();
})
.WithValidation<RegisterRequest>() // включаем фильтр
.Produces(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status422UnprocessableEntity);

```