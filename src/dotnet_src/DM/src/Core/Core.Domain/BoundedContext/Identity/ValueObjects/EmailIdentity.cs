using Core.Domain.SharedKernel.ValueObjects;
using Core.Domain.SharedKernel.ValueObjects.Abstractions;

namespace Core.Domain.BoundedContext.Identity.ValueObjects;

public class EmailIdentity : Email 
{
    public EmailIdentity(string value, 
        bool emailConfirmedStatus = false, 
        DateTimeOffset? emailConfirmedDateChanged = null, 
        DateTimeOffset? emailConfirmedCodeDateCreated = null, 
        string? emailConfirmedCode = null, 
        DateTimeOffset? emailConfirmedCodeDateExpired = null) : base(value)
    {
        EmailConfirmedStatus = emailConfirmedStatus;
        EmailConfirmedDateChanged = emailConfirmedDateChanged;
        EmailConfirmedCodeDateCreated = emailConfirmedCodeDateCreated;
        EmailConfirmedCode = emailConfirmedCode;
        EmailConfirmedCodeDateExpired = emailConfirmedCodeDateExpired;
    }

    public bool EmailConfirmedStatus { get;  }
    public DateTimeOffset? EmailConfirmedDateChanged { get;  }

    public DateTimeOffset? EmailConfirmedCodeDateCreated { get;  }
    public string? EmailConfirmedCode { get;  } 
    public DateTimeOffset? EmailConfirmedCodeDateExpired { get;  }
}