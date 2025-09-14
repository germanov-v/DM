using Core.Domain.SharedKernel.ValueObjects;
using Core.Domain.SharedKernel.ValueObjects.Abstractions;
using Core.Domain.SharedKernel.ValueObjects.Base;

namespace Core.Domain.BoundedContext.Identity.ValueObjects;

public sealed class PhoneIdentity : Phone
{
    public bool PhoneConfirmedStatus { get;  }

    public DateTimeOffset? PhoneConfirmedDateChanged { get;  }

    public string? PhoneConfirmedCode { get;  }

    public DateTimeOffset? PhoneConfirmedCodeDateCreated { get;  }

    public DateTimeOffset? PhoneConfirmedCodeDateExpired { get;  }
    
    
    public PhoneIdentity(string value, bool phoneConfirmedStatus, DateTimeOffset? phoneConfirmedDateChanged, string? phoneConfirmedCode, DateTimeOffset? phoneConfirmedCodeDateCreated, DateTimeOffset? phoneConfirmedCodeDateExpired) 
        : base(value)
    {
        PhoneConfirmedStatus = phoneConfirmedStatus;
        PhoneConfirmedDateChanged = phoneConfirmedDateChanged;
        PhoneConfirmedCode = phoneConfirmedCode;
        PhoneConfirmedCodeDateCreated = phoneConfirmedCodeDateCreated;
        PhoneConfirmedCodeDateExpired = phoneConfirmedCodeDateExpired;
    }
}