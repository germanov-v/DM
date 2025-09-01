using Core.Application.Common.Results;
using Core.Application.Dto.Identity;
using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.SharedKernel.Abstractions;

namespace Core.Application.Handlers.Identity;

public interface IIdentityHandler : IApplicationService
{
    Task<Result<AuthJwtResponseDto>> Authenticate(LoginEmailRoleFingerprintRequestDto dto, 
        CancellationToken cancellationToken,
        string? refreshToken =  null);

    Task<AuthJwtResponseDto> RefreshAuth(RefreshTokenDto dto, CancellationToken cancellationToken,
        string? refreshToken  =  null);


    // Task<User> RegisterUserByEmail(LoginEmailRequestDto dto, string[] roleAlias,
    //     CancellationToken cancellationToken, User userEntity = null );

    Task<User?> GetUserByGuidId(Guid guid, CancellationToken cancellationToken);
    
    Task<User> GetUserByDto(LoginEmailRoleFingerprintRequestDto dto, 
        CancellationToken cancellationToken
    );
}