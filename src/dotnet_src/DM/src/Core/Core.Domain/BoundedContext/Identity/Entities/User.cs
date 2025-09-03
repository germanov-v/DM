using Core.Domain.BoundedContext.Identity.Events;
using Core.Domain.BoundedContext.Identity.Repositories;
using Core.Domain.BoundedContext.Identity.ValueObjects;
using Core.Domain.SharedKernel.Abstractions;
using Core.Domain.SharedKernel.Entities;
using Core.Domain.SharedKernel.ValueObjects;

namespace Core.Domain.BoundedContext.Identity.Entities;

public class User : EntityRoot<IdGuid>
{
    public Email? Email { get; }

    public Phone? Phone { get;  }
    
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
        

    private User(bool isActive, Name name, IEnumerable<Role>? roles)
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
    }

    public User(IdGuid idGuid, Email email, bool isActive, Name name, IEnumerable<Role>? roles = null)
        : this(isActive, name, roles)
    {
        Id = idGuid;
        Email = email ?? throw new ArgumentNullException(nameof(email));
    }

    public User(IdGuid idGuid, Phone phone, bool isActive, Name name, IEnumerable<Role>? roles = null)
        : this(isActive, name, roles)
    {
        Id = idGuid;
        Phone = phone ?? throw new ArgumentNullException(nameof(phone));
    }


    public void RegisterUserByEmail(string email,
        string password)
    {
        AddDomainEvent(new UserRegisterByEmail());
    }


    public void RegisterUserByPhone(string email, string password)
    {
    }
    // public DateTimeOffset CreatedDate { get; set; }  = DateTimeApplication.GetCurrentDate();
    // public DateTimeOffset UpdatedDate { get; set; }  = DateTimeApplication.GetCurrentDate();
    //
    // public ICollection<Role> Roles { get; set; } = new List<Role>();


    public bool BlockedStatus { get; set; }

    public DateTimeOffset? BlockedDate { get; set; }

    public bool EmailConfirmedStatus { get; set; }
    public DateTimeOffset? EmailConfirmedDateChanged { get; set; }

    public DateTimeOffset? EmailConfirmedCodeDateCreated { get; set; }
    public string EmailConfirmedCode { get; set; } = null!;
    public DateTimeOffset? EmailConfirmedCodeDateExpired { get; set; }


    public bool PhoneConfirmedStatus { get; set; }

    public DateTime? PhoneConfirmedDateChanged { get; set; }

    public string PhoneConfirmedCode { get; set; } = null!;

    public DateTimeOffset? PhoneConfirmedCodeDateCreated { get; set; }

    public DateTimeOffset? PhoneConfirmedCodeDateExpired { get; set; }


    #region permissions

    public bool AllowedToCreateBlogStatus { get; set; }

    public DateTimeOffset AllowedToCreateBlogDate { get; set; }

    #endregion
}