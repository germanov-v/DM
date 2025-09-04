using Core.Domain.SharedKernel.ValueObjects.Abstractions;
using Core.Domain.SharedKernel.ValueObjects.Base;

namespace Core.Domain.BoundedContext.Identity.ValueObjects;

public class YandexId : ValueObject<string>
{
    public YandexId(string value) : base(value)
    {
    }
}