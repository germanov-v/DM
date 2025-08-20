using Core.Application.Models.Identity;

namespace Core.Application.Abstractions.Services;

public interface IAuthProvider
{
    public Task<ExternalIdentityResult> Authenticate();
}