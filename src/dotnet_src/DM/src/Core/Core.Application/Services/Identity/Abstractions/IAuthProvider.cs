using Core.Application.Models.Identity;

namespace Core.Application.Abstractions.Services.Identity;


/// <summary>
/// Провайдеры: Email, Phone, Vk, Yandex
/// </summary>
public interface IAuthProvider : IApplicationService
{
    
    
    public Task<ExternalIdentityResult> Authenticate();
    
  
    
}