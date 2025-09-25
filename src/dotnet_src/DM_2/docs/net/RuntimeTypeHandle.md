`RuntimeTypeHandle` в .NET — это **специальная структурка-обёртка над указателем на внутренние метаданные типа**, которые CLR хранит в памяти.

Говоря проще: у каждого загруженного типа в рантайме есть свой низкоуровневый дескриптор (`method table` / `EEType` в CLR). `RuntimeTypeHandle` — это **типобезопасный "хендл" (ручка) на эту таблицу**.

---

### Где используется

* Каждый `System.Type` внутри себя содержит `RuntimeTypeHandle` → именно через него `Type` знает, какой конкретный тип в CLR он описывает.
* Используется в низкоуровневых API Reflection и JIT.
* Можно получить напрямую:

  ```csharp
  Type t = typeof(string);
  RuntimeTypeHandle handle = t.TypeHandle;
  Console.WriteLine(handle); // что-то вроде "System.RuntimeTypeHandle"
  ```
* И обратно:

  ```csharp
  Type t2 = Type.GetTypeFromHandle(handle);
  Console.WriteLine(t2 == typeof(string)); // True
  ```

---

### Что внутри

* На практике `RuntimeTypeHandle` хранит **IntPtr**, указывающий на `MethodTable` (в CoreCLR) или аналог в CoreRT/Mono.
* CLR гарантирует, что этот указатель уникален для каждого типа и существует всё время, пока тип загружен в AppDomain.

---

### Отличие от `Type`

* `Type` — это высокоуровневый объект .NET, с кучей методов (`GetMethods`, `GetProperties`, и т.п.).
* `RuntimeTypeHandle` — лёгкая структурка с «сырым» указателем.

    * Можно быстро сравнивать два типа (сравнение хендлов — дешевле, чем `Type.Equals`).
    * Может быть полезен в низкоуровневом коде (серилизаторы, компиляторы выражений, runtime-библиотеки).

---

### Пример

```csharp
using System;

class Program
{
    static void Main()
    {
        RuntimeTypeHandle h1 = typeof(int).TypeHandle;
        RuntimeTypeHandle h2 = typeof(string).TypeHandle;

        Console.WriteLine(h1.Equals(h2)); // False

        Type t = Type.GetTypeFromHandle(h1);
        Console.WriteLine(t.FullName); // System.Int32
    }
}
```

---

👉 Итог:
`RuntimeTypeHandle` — это **управляемый дескриптор на внутренние данные CLR о типе**, используемый как низкоуровневый идентификатор типа. Это быстрый и безопасный способ «подержать за ручку» реальный runtime-тип, не держа тяжёлый объект `Type`.

---

Хочешь, я покажу на ассемблере/IL, как `typeof(T).TypeHandle` реально подставляет указатель на `MethodTable` и чем это быстрее, чем работать через `Type`?
