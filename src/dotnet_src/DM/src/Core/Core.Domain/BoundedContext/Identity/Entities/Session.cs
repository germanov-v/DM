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
        AppDate createdAt,
        AppDate refreshTokenExpiresAt,
        IPAddress? ip, string? fingerprint, AuthProvider authProvider, IdGuid? id=null)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        UserId = userId;
        CreateAt = createdAt;
        RefreshTokenExpiresAt = refreshTokenExpiresAt;
        Ip = ip;
        Fingerprint = fingerprint;
        AuthProvider = authProvider;
        if (id != null)
            Id = id;
    }


    public string AccessToken { get; }

    public string RefreshToken { get; }

    public AuthProvider AuthProvider { get; }

    public long UserId { get; }

    public AppDate CreateAt { get; }

    public AppDate RefreshTokenExpiresAt { get; }

    public string? Fingerprint { get; }

    public IPAddress? Ip { get; }
}