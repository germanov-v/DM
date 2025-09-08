using Core.Domain.BoundedContext.Identity.Events;
using Core.Domain.BoundedContext.Identity.Repositories;
using Core.Domain.BoundedContext.Identity.ValueObjects;
using Core.Domain.SharedKernel.Abstractions;
using Core.Domain.SharedKernel.Entities;
using Core.Domain.SharedKernel.ValueObjects;

namespace Core.Domain.BoundedContext.Identity.Entities;

public class User : EntityRoot<IdGuid>
{
    public EmailIdentity? Email { get; }
    
    public Password? Password { get; }

    public PhoneIdentity? Phone { get;  }
    
    public VkId? VkId { get;  }
    
    public YandexId? YandexId { get; }

    public Name Name { get; private set; }

    public bool IsActive { get; private set; }

    public bool IsRegisterByPhone { get; private set; }


    private readonly HashSet<Role> _roles;

    public IReadOnlyCollection<Role> Roles => _roles;

    // public IEnumerable<Role>? Roles { get; private set; }


    public string Contact => Phone?.Value
                             ?? Email?.Value
                             ?? VkId?.Value
                             ?? YandexId?.Value
                             ?? throw new InvalidOperationException("Contact cannot be null.");
        

    private User(bool isActive, Name name,  IEnumerable<Role>? roles, IdGuid? id)
    {
        IsActive = isActive;
        // if (roles != null)
        // {
        //     _roles = [..roles];
        //   
        // }

        _roles = roles switch
        {
            null => new HashSet<Role>(),
            HashSet<Role> hs => new HashSet<Role>(hs), // быстрая копия
            ICollection<Role> c => new HashSet<Role>(c), // .NET сам оптимизирует под Count
            _ => new HashSet<Role>(roles)
        };

        Name = name;
        
        if (id != null)
            Id = id;
    }

    public User( EmailIdentity email, bool isActive, Name name,
         IEnumerable<Role>? roles = null,IdGuid? id=null)
        : this(isActive, name, roles,id)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
    }

    public User( EmailIdentity email, Password password, bool isActive, Name name,
        IEnumerable<Role>? roles = null,IdGuid? id=null)
        : this(isActive, name, roles,id)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Password = password ?? throw new ArgumentNullException(nameof(password));
    }

    public User(PhoneIdentity phone, bool isActive, Name name, IEnumerable<Role>? roles = null,
        IdGuid? id=null)
        : this(isActive, name, roles,id)
    {
        Phone = phone ?? throw new ArgumentNullException(nameof(phone));
    }


    public void AddRole(Role role)
    {
        _roles.Add(role);
    }
    
    
    
    //

    public void RegisterUserByEmail(string email,
        string password)
    {
        AddDomainEvent(new UserRegisterByEmail());
    }


    public void RegisterUserByPhone(string email, string password)
    {
    }
  
    
    


    public bool BlockedStatus { get; set; }

    public DateTimeOffset? BlockedDate { get; set; }







    #region permissions

    public bool AllowedToCreateBlogStatus { get; set; }

    public DateTimeOffset AllowedToCreateBlogDate { get; set; }

    #endregion
}