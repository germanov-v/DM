using Core.Domain.SharedKernel.ValueObjects.Abstractions;

namespace Core.Domain.SharedKernel.ValueObjects;

public class FileApp : ValueObject
{
    
    public long Id { get; private set; }
    public Guid GuidId { get; private set; }
    
    
    
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}