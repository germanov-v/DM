using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Core.Application.Abstractions.Services;
using Core.Application.Abstractions.Services.Identity;
using Core.Application.Options.Identity;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Core.Infrastructure.Services.Identity;

public class CryptoIdentityService(IOptions<IdentityAuthOptions> authOption)  : ICryptoIdentityService
{
    private readonly IdentityAuthOptions _authOptions = authOption.Value;


    public byte[] CreateSalt()
        => RandomNumberGenerator.GetBytes(128 / 8);

    public string GetSaltStr(byte[] salt) => Convert.ToBase64String(salt);

    public byte[] GetSaltBytes(string salt) => Convert.FromBase64String(salt);

    public string GetHashPassword(string password, byte[] salt) => Convert.ToBase64String(
        KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8
        )
    );


    public string GenerateAccessToken(IEnumerable<Claim> claims, DateTime expiresRefresh)
    {
    
        var jwtToken = new JwtSecurityToken(
            issuer: _authOptions.Url,
            audience: _authOptions.Url,
            claims: claims,
            //  notBefore:
            expires: expiresRefresh,
            signingCredentials: new SigningCredentials(
                GetSymmetricSecurityKey(),
                SecurityAlgorithms.HmacSha256
            )
        );

        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }



    public string GenerateRefreshToken() => Guid.NewGuid().ToString();

    private SymmetricSecurityKey GetSymmetricSecurityKey() =>
        new(Encoding.UTF8.GetBytes(_authOptions.CryptoKey));
}