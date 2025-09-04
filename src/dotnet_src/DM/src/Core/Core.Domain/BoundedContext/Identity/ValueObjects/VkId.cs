using Core.Domain.SharedKernel.ValueObjects.Abstractions;
using Core.Domain.SharedKernel.ValueObjects.Base;

namespace Core.Domain.BoundedContext.Identity.ValueObjects;

public class VkId : ValueObject<string>
{
    public VkId(string value) : base(value)
    {
    }
}