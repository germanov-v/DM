using Core.Application.Models.Identity;
using Core.Domain.SharedKernel.Abstractions;

namespace Core.Application.Abstractions.Services.Identity;


/// <summary>
/// Провайдеры: Email, Phone, Vk, Yandex
/// </summary>
public interface IAuthProvider : IApplicationService
{
    
    
    public Task<ExternalIdentityResult> Authenticate();
    
  
    
}