using System.Security.Claims;
using Core.Domain.SharedKernel.Abstractions;

namespace Core.Application.Abstractions.Services.Identity;

public interface ICryptoIdentityService : IApplicationService
{
    public byte[] CreateSalt();

    public string GetSaltStr(byte[] salt) ;

    byte[] GetSaltBytes(string salt);

    public string GetHashPassword(string password, byte[] salt) ;

    string GenerateAccessToken(IEnumerable<Claim> claims);

    string GenerateRefreshToken();
}