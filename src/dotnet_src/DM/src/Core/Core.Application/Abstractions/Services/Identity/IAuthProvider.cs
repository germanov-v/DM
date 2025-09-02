using Core.Application.Models.Identity;
using Core.Domain.SharedKernel.Abstractions;

namespace Core.Application.Abstractions.Services.Identity;

public interface IAuthProvider : IApplicationService
{
    public Task<ExternalIdentityResult> Authenticate();
    
  
    
}