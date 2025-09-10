using System.Net;
using Core.Domain.BoundedContext.Identity.ValueObjects;
using Core.Domain.SharedKernel.Entities;
using Core.Domain.SharedKernel.ValueObjects;

namespace Core.Domain.BoundedContext.Identity.Entities;

public class Session : EntityRoot<IdGuid>
{
    public Session(string accessToken,
        string refreshToken,
        long userId,
        DateTimeOffset createdAt,
        DateTimeOffset refreshExpired,
        IPAddress? ip, string fingerprint, AuthProvider authProvider)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        UserId = userId;
        CreateAt = createdAt;
        RefreshExpired = refreshExpired;
        Ip = ip;
        Fingerprint = fingerprint;
        AuthProvider = authProvider;
    }

    public string AccessToken { get; }

    public string RefreshToken { get; }

    public AuthProvider AuthProvider { get; }

    public long UserId { get; }

    public DateTimeOffset CreateAt { get; }

    public DateTimeOffset RefreshExpired { get; }

    public string Fingerprint { get; }

    public IPAddress? Ip { get; }
}