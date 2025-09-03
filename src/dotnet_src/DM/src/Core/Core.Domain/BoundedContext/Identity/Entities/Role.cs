using Core.Domain.SharedKernel.Entities;
using Core.Domain.SharedKernel.ValueObjects;

namespace Core.Domain.BoundedContext.Identity.Entities;

public class Role : EntityRoot<Id>
{
    public Role(string title, string alias)
    {
        Title = title;
        Alias = alias;
    }

    public string Title { get; }
    
    public string Alias { get; }
}