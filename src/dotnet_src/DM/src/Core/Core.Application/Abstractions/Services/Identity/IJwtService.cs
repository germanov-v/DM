using Core.Application.Dto.Identity;
using Core.Domain.BoundedContext.Identity.Entities;

namespace Core.Application.Abstractions.Services.Identity;

public interface IJwtService : IApplicationService
{
    AuthJwtResponseDto GenerateJwtService(User user);
}