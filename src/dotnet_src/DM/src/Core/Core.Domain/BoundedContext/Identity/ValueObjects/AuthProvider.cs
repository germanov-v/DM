using Core.Domain.SharedKernel.ValueObjects.Abstractions;

namespace Core.Domain.BoundedContext.Identity.ValueObjects;

public sealed class AuthProvider : ValueObject
{
    public static readonly AuthProvider Email = new(1, "Email");
    public static readonly AuthProvider Phone = new(1, "Phone");
    public static readonly AuthProvider Vk = new(1, "Vk");
    public static readonly AuthProvider Yandex = new(1, "Yandex");
    public static readonly AuthProvider Apple = new(1, "Apple");


    public int Code { get; }
    public string Name { get; }


    private AuthProvider(int code, string name)
    {
        Code = code;
        Name = name;
    }

    public override string ToString() => Name;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Email;
        yield return Phone;
        yield return Vk;
        yield return Yandex;
        yield return Apple;
    }
}