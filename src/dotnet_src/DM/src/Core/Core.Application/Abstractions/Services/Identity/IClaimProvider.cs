using System.Security.Claims;
using Core.Application.Dto.Identity;
using Core.Domain.BoundedContext.Identity.Entities;

namespace Core.Application.Abstractions.Services.Identity;

public interface IClaimProvider : IApplicationService
{
 //    GenerateJwtService(User user);
     List<Claim> GetClaims(User user);
}