using Core.Domain.SharedKernel.Entities;
using Core.Domain.SharedKernel.ValueObjects;

namespace Core.Domain.BoundedContext.Identity.Entities;

public class Role : EntityRoot<IdGuid>
{
    
    public Role(string name, string alias) 
    {
        Name = name;
        Alias = alias;
    }
    
    public Role(IdGuid id,string title, string alias) : this(title, alias)
    {
        Id = id;
    }

    public string Name { get; }
    
    public string Alias { get; }
}