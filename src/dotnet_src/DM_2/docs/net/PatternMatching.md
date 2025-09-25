Окей 🚀 — вот тебе **сводная таблица паттерн-матчинга в C# (от C# 7 до C# 12)**.

---

## 🔹 Основные виды паттернов

| Вид паттерна              | Пример                                             | Что делает                                      |
| ------------------------- | -------------------------------------------------- | ----------------------------------------------- |
| **Constant pattern**      | `x is 5`                                           | Проверка на равенство с константой              |
| **Type pattern**          | `if (o is string s)`                               | Проверка типа + объявление переменной           |
| **Var pattern**           | `if (o is var x)`                                  | Всегда истина, просто "достаёт" значение        |
| **Discard pattern**       | `if (o is _)`                                      | Игнорируем значение (wildcard)                  |
| **Relational pattern**    | `if (x is > 0 and < 100)`                          | Проверка через операторы (`<`, `>`, `<=`, `>=`) |
| **Logical pattern**       | `if (x is > 0 and < 100 or 200)`                   | Комбинации `and`, `or`, `not`                   |
| **Property pattern**      | `if (p is { Age: > 18, Name: "Tom" })`             | Проверка по свойствам объекта                   |
| **Positional pattern**    | `if (pt is (0, 0))`                                | Сопоставление с `Deconstruct` (tuple / record)  |
| **Recursive pattern**     | `if (o is Person { Address: { City: "Moscow" } })` | Вложенные property-паттерны                     |
| **List pattern (C# 11)**  | `if (arr is [1, 2, .., 10])`                       | Сопоставление с массивами/списками              |
| **Slice pattern (C# 11)** | `if (arr is [.., 10])`                             | Проверка последнего элемента                    |
| **Parenthesized pattern** | `if (x is (> 0 and < 10) or 100)`                  | Группировка условий                             |
| **Pattern combinators**   | `not`, `and`, `or`                                 | Логические связки паттернов                     |

---

## 🔹 Примеры на коде

### 1. Constant pattern

```csharp
if (day is "Monday")
    Console.WriteLine("Начало недели");
```

---

### 2. Type pattern

```csharp
if (obj is string s)
    Console.WriteLine($"Строка длиной {s.Length}");
```

---

### 3. Relational + logical pattern

```csharp
if (x is >= 0 and <= 100 or 200)
    Console.WriteLine("x в диапазоне 0..100 или равно 200");
```

---

### 4. Property pattern

```csharp
if (user is { Age: > 18, Name: "Alex" })
    Console.WriteLine("Совпало по свойствам");
```

---

### 5. Recursive pattern

```csharp
if (order is { Customer: { Address: { City: "Berlin" } } })
    Console.WriteLine("Заказчик из Берлина");
```

---

### 6. Positional pattern

```csharp
record Point(int X, int Y);

var p = new Point(0, 0);

if (p is (0, 0))
    Console.WriteLine("Это начало координат");
```

---

### 7. List / Slice patterns (C# 11+)

```csharp
int[] nums = [1, 2, 3, 4, 5];

if (nums is [1, 2, .., 5])
    Console.WriteLine("Начинается с 1,2 и заканчивается на 5");

if (nums is [.., 5])
    Console.WriteLine("Последний элемент равен 5");
```

---

### 8. Var / Discard

```csharp
if (o is var x) // всегда true
    Console.WriteLine($"o = {x}");

if (o is _) // всегда true, но без переменной
    Console.WriteLine("o есть, но нам не важно, что это");
```

---

⚡️ Итого: паттерн-матчинг в C# — это не только `is string s`, а целый язык для описания "шаблонов" значений.
В твоём примере `HashSet<Role> hs => …` — это **type pattern**, но можно комбинировать его с `when`, `property`, `list`, и т.д.

---

Хочешь, я соберу **мини-шпаргалку с 1 строкой на каждый паттерн**, чтобы её можно было держать прямо в проекте (например, в комментариях)?
