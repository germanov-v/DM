using Core.Domain.SharedKernel.ValueObjects.Abstractions;

namespace Core.Domain.BoundedContext.Identity.ValueObjects;

public sealed class AuthProvider : ValueObject
{
    public static readonly AuthProvider Email = new(1, "Email");
    public static readonly AuthProvider Phone = new(2, "Phone");
    public static readonly AuthProvider Vk = new(3, "Vk");
    public static readonly AuthProvider Yandex = new(4, "Yandex");
    public static readonly AuthProvider Apple = new(5, "Apple");


    private static readonly Dictionary<string, AuthProvider> _allowValues = new(StringComparer.OrdinalIgnoreCase)
    {
        [Email.Alias]=Email,
        [Phone.Alias]=Phone,
        [Vk.Alias]=Email,
        [Yandex.Alias]=Yandex,
        [Apple.Alias]=Apple,
    };
    public int Code { get; }
    public string Alias { get; }


    private AuthProvider(int code, string alias)
    {
        Code = code;
        Alias = alias;
     
    }
    
    public static AuthProvider GetValue( string name)=> _allowValues.TryGetValue(name, out var value) ? value : 
        throw new KeyNotFoundException($"{name} in {nameof(AuthProvider)} not found");

    public override string ToString() => Alias;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Code;
        yield return Alias;
    }
}