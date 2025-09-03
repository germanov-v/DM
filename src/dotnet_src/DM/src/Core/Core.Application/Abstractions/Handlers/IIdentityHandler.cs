using System.Net;
using Core.Application.Abstractions.Services;
using Core.Application.Common.Results;
using Core.Application.Dto.Identity;
using Core.Domain.BoundedContext.Identity.Entities;

namespace Core.Application.Abstractions.Handlers;

public interface IIdentityHandler : IApplicationService
{
    Task<Result<AuthJwtResponseDto>> AuthenticateByEmailPasswordRole(string email,
        string password, 
        string role,
        IPAddress? ip,
        string fingerprint,
        CancellationToken cancellationToken);

    Task<AuthJwtResponseDto> RefreshAuth(RefreshTokenDto dto, CancellationToken cancellationToken,
        string? refreshToken  =  null);


    // Task<User> RegisterUserByEmail(LoginEmailRequestDto dto, string[] roleAlias,
    //     CancellationToken cancellationToken, User userEntity = null );

    Task<User?> GetUserByGuidId(Guid guid, CancellationToken cancellationToken);
    
    Task<User> GetUserByDto(LoginEmailRoleFingerprintRequestDto dto, 
        CancellationToken cancellationToken
    );
}