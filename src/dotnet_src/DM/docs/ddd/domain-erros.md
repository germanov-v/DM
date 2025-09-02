Супер, давай разведём понятия и покажу «каноничный» минимализм.

## Что такое DomainError (и почему без оверхеда)

**Цель домена** — выразить нарушение **правила модели**, без привязки к HTTP, БД, FluentValidation и т.п.
Поэтому `DomainError` должен быть крошечным и нейтральным:

```csharp
// Core.Domain.SharedKernel/Errors/DomainError.cs
public readonly record struct DomainError(string Code, string Message, params object[] Args);
```

* **Только Code и Message** (и опционально `Args` для локализации).
* **Никаких ErrorType, HTTP-статусов, ProblemDetails** — это уже уровень Application/API.
* Оверхед минимальный: `record struct` (value type), без аллокаций в куче при возврате.

Если хочешь ещё суше — можно хранить **только Code**, а Message формировать на уровне API по локали. Но большинству удобнее оставить и `Message` по умолчанию.

---

## Примеры доменных ошибок (Identity)

```csharp
// Core.Domain.BoundedContext.Identity/Errors/IdentityErrors.cs
namespace Core.Domain.BoundedContext.Identity.Errors;

public static class IdentityErrors
{
    // это правило модели (валидация значения/состояния), домен может вернуть его сам
    public static DomainError EmailEmpty()
        => new("identity.email_empty", "Email must not be empty.");

    public static DomainError EmailInvalidFormat(string value)
        => new("identity.email_invalid_format", $"Email '{value}' has invalid format.", value);

    public static DomainError PasswordTooWeak()
        => new("identity.password_too_weak", "Password does not meet complexity requirements.");

    public static DomainError UserLocked()
        => new("identity.user_locked", "User is locked.");

    public static DomainError RoleNotAssigned(string role)
        => new("identity.role_not_assigned", $"Role '{role}' is not assigned to user.", role);

    // ⚠️ Уникальность email — это не внутренняя инварианта агрегата,
    // а кросс-агрегатное/хранилищное правило ⇒ обычно Application.
    // Поэтому в домене так ошибку НЕ объявляем (или объявляем как общий код без решения о типе).
    public static DomainError EmailAlreadyUsed(string value)
        => new("identity.email_not_unique", $"Email '{value}' is already used.", value);
}
```

### Где их использовать в домене

```csharp
public DomainResult<Email> TrySetEmail(string raw)
{
    if (string.IsNullOrWhiteSpace(raw))
        return DomainResult<Email>.Fail(IdentityErrors.EmailEmpty());

    if (!Email.IsValid(raw))
        return DomainResult<Email>.Fail(IdentityErrors.EmailInvalidFormat(raw));

    var email = new Email(raw);
    _email = email;
    return DomainResult<Email>.Ok(email);
}

public DomainResult<Unit> EnsureRole(string role)
{
    if (!Roles.Contains(role))
        return DomainResult<Unit>.Fail(IdentityErrors.RoleNotAssigned(role));

    return DomainResult<Unit>.Ok(Unit.Value);
}
```

> Здесь **нет** `ErrorType`. Домен сообщает только, **какое правило нарушено**.

---

## Как связать DomainError с ошибками Application

В **Application** решаем, как трактовать доменный код: это валидация? конфликт? запрет?
Делаем маппер один раз, централизованно:

```csharp
// Core.Application.Common/Errors/Error.cs
public enum ErrorType { Validation, NotFound, Conflict, Forbidden, Failure }
public sealed record Error(string Code, string Message, ErrorType Type);

// Core.Application.Common/Errors/DomainErrorMapping.cs
public static class DomainErrorMapping
{
    public static Error ToAppError(this DomainError e) =>
        e.Code switch
        {
            // всё про формат/пустоту — это Validation (422)
            "identity.email_empty"           => new(e.Code, e.Message, ErrorType.Validation),
            "identity.email_invalid_format"  => new(e.Code, e.Message, ErrorType.Validation),
            "identity.password_too_weak"     => new(e.Code, e.Message, ErrorType.Validation),

            // отсутствие роли — чаще Forbidden (403)
            "identity.role_not_assigned"     => new(e.Code, e.Message, ErrorType.Forbidden),

            // уникальность — это уже решение уровня Application:
            // если правило проверено приложением без исключений → Conflict (409)
            "identity.email_not_unique"      => new(e.Code, e.Message, ErrorType.Conflict),

            // по умолчанию — Failure (400/500 в зависимости от политики)
            _ => new(e.Code, e.Message, ErrorType.Failure)
        };
}
```

> Хочешь — раздели на несколько функций `ToValidation()`, `ToConflict()` и выбирай явно в хендлере; или держи таблицу соответствий тут, как выше.

### Пример в хендлере

```csharp
// доменная операция
var dr = user.TrySetEmail(dto.Email);
if (!dr.IsSuccess)
    return Result<User>.Fail(dr.Error!.Value.ToAppError()); // DomainError → Error (с ErrorType)


// проверка уникальности — уже Application (через репо)
if (!await _users.IsEmailUniqueAsync(dto.Email, ct))
    return Result<User>.Fail(IdentityErrors.EmailAlreadyUsed(dto.Email).ToAppError());
```

---

## Где NotFound?

`NotFound` — обычно **не доменная** ошибка, а результат поиска (репозитория).
Возвращай `Maybe<T>`/`T?`, а в Application маппь в `ErrorType.NotFound`.

```csharp
var user = await _users.FindByEmailAsync(dto.Email, ct);
if (user is null)
    return Result<User>.Fail(new Error("identity.user_not_found", "User not found.", ErrorType.NotFound));
```

---

## Нужен ли вообще DomainError? (про «оверхед»)

Это **минимальный** инструмент, чтобы писать **не-исключающие** методы в домене (Try…/Ensure…).
Альтернативы:

* **Бросать исключения** в домене → дорого, шумно, тяжело тестировать; ты этого не хочешь.
* **Возвращать bool и string message** → теряешь стабильный `Code`, сложнее локализовать и маппить.
* **Возвращать enum** → коды начнут плодиться, сообщений не будет; всё равно потом придётся где-то хранить тексты.

`record struct DomainError` решает всё: крошечный, быстрый, несёт `Code` (ключ для локализации/аналитики) и `Message` по умолчанию.
А «склейку» с прикладным `ErrorType` ты делаешь **одним местом** в Application — и забываешь на уровне домена.

---

## TL;DR

* **DomainError** — маленький value-type с `Code` и `Message`. Без `ErrorType`. Это **не оверхед**, а удобный способ писать Try-методы в домене **без исключений**.
* В домене объявляй только **правила модели** (формат, пустота, состояние, роли).
  Уникальность/NotFound — чаще application-concerns.
* В **Application** делай **централизованный маппинг** `DomainError → Error(ErrorType)` — так ты «связываешь» доменные ошибки с прикладной семантикой (422/409/403/…).
* В хендлерах: `DomainResult<T>` → `Result<T>` через этот маппер; инфраструктурные сбои остаются исключениями для middleware.
