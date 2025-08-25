using Core.Domain.BoundedContext.Identity.Events;
using Core.Domain.BoundedContext.Identity.Repositories;
using Core.Domain.SharedKernel.Abstractions;
using Core.Domain.SharedKernel.Entities;
using Core.Domain.SharedKernel.ValueObjects;

namespace Core.Domain.BoundedContext.Identity.Entities;

public class User : EntityRoot<IdGuid>
{
    public IdGuid IdGuid { get; private set; }
    
    public string Email { get; private set; }
    
    
    
    public bool IsActive {get; private set;}
    
    public bool IsRegisterByPhone { get; private set; }


    public User(IdGuid idGuid, string email)
    {
        IdGuid = idGuid;
        Email = email;
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