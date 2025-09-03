using System.Net;
using Core.Domain.SharedKernel.Entities;
using Core.Domain.SharedKernel.ValueObjects;

namespace Core.Domain.BoundedContext.Identity.Entities;

public class Session : EntityRoot<IdGuid>
{
    public Session(string accessToken, 
        string refreshToken, 
        long userId, 
        DateTimeOffset dateCreated, 
        DateTimeOffset refreshExpired, 
        IPAddress? ip, string fingerprint)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        UserId = userId;
        DateCreated = dateCreated;
        RefreshExpired = refreshExpired;
        Ip = ip;
        Fingerprint = fingerprint;
    }

    public string AccessToken { get;  }
    
    public string RefreshToken { get; }
    
    public long UserId { get;  }
    
    public DateTimeOffset DateCreated { get;  }
    
    public DateTimeOffset RefreshExpired { get;  }
    
    public string Fingerprint { get; }
    
    public IPAddress? Ip { get; }
}