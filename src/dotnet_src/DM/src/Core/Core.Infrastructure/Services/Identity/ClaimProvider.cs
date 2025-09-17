using System.Security.Claims;
using Core.Application.Abstractions.Services.Identity;
using Core.Domain.BoundedContext.Identity.Entities;

namespace Core.Infrastructure.Services.Identity;


/// <summary>
/// TODO: этот сервис точно нужен?
/// </summary>
public class ClaimProvider : IClaimProvider
{
    
    // private readonly ICryptoIdentityService _cryptoService;
    // private readonly IdentityAuthOptions _authOptions;
    //
    // public ClaimProvider(ICryptoIdentityService cryptoService, IOptions<IdentityAuthOptions> authOptions)
    // {
    //     _cryptoService = cryptoService;
    //     _authOptions = authOptions.Value;
    // }
    //
    // public AuthJwtResponseDto GenerateJwtService(User user)
    // {
    //     if (user == null)
    //         throw new ArgumentNullException(nameof(user));
    //     if (user.Roles is not { } || user.Roles.Count == 0)
    //         throw new InvalidOperationException($"User.Roles is null! User: {user.Id.ValueGuid}");
    //
    //     if (user.Roles.Count == 0)
    //         throw new InvalidOperationException($"User.Roles is empty! User: {user.Id.ValueGuid}");
    //     var claims =   user.Roles.SelectMany(p => new[]
    //     {
    //         new Claim(ClaimTypes.Role, p.Alias),
    //     }).ToList();
    //
    //    // var identityContact = user.Email ?? user.Phone;
    //    // claims.Add(new Claim(IdentityAuthConstants.NameClaim, identityContact));
    //     claims.Add(new Claim(IdentityAuthConstants.UserIdClaim, user.Id.ValueGuid.ToString()));
    //
    //
    //
    //    
    //     var userAuthModel = new AuthUserResponseDto(
    //         user.Id.ValueGuid.ToString(),
    //         user.Name.Value ?? String.Empty,
    //         user.Roles.Select(p => p.Alias).ToList(),
    //         user.Contact);
    //         
    //         
    //         
    //
    //     var result = new AuthJwtResponseDto(
    //         accessToken,
    //         refreshToken,
    //         _authOptions.AccessTokenLifetime,
    //         userAuthModel
    //     );
    //     
    //
    //     return result;
    // }


    public List<Claim> GetClaims(User user)=>user.Roles.SelectMany(p => new[]
    {
        new Claim(ClaimTypes.Role, p.Alias),
    }).ToList();
   
}