using System.Globalization;
using System.Net;
using System.Security.Authentication;
using System.Security.Claims;
using Core.Application.Abstractions;
using Core.Application.Abstractions.BusinessLogic.Identity;
using Core.Application.Abstractions.Handlers;
using Core.Application.Abstractions.Services;
using Core.Application.Abstractions.Services.Identity;
using Core.Application.Common.Results;
using Core.Application.Dto.Identity;
using Core.Application.Extensions;
using Core.Application.Handlers.Identity.Errors;
using Core.Application.Options;
using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.BoundedContext.Identity.Repositories;
using Core.Domain.BoundedContext.Identity.ValueObjects;
using Core.Domain.SharedKernel.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Core.Application.Handlers.Identity;

public class IdentityHandler : IIdentityHandler
{
    private readonly ICryptoIdentityService _cryptoIdentityService;
    private readonly IEmailPasswordUserProvider _emailPasswordUserProvider;
    private readonly ISessionService _sessionService;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IdentityAuthOptions _authOption;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<IdentityHandler> _logger;
    private readonly IClaimProvider _claimProvider;
    private readonly IDateTimeProvider _dateTimeProvider;

    public IdentityHandler(
        IOptions<IdentityAuthOptions> authOption,
        ISessionRepository authTokenRepository,
        ICryptoIdentityService cryptoIdentityService,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        IEmailPasswordUserProvider emailPasswordUserProvider, ILogger<IdentityHandler> logger,
        IClaimProvider claimProvider, IDateTimeProvider dateTimeProvider, ISessionService sessionService)
    {
        _authOption = authOption.Value;
        _cryptoIdentityService = cryptoIdentityService;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _emailPasswordUserProvider = emailPasswordUserProvider;
        _logger = logger;
        _claimProvider = claimProvider;
        _dateTimeProvider = dateTimeProvider;
        _sessionService = sessionService;
    }

    public async Task<Result<AuthJwtResponseDto>> AuthenticateByEmailPasswordRole(string email,
        string password,
        string role,
        IPAddress? ip,
        string fingerprint,
        CancellationToken cancellationToken)
    {
        var resultUser = await _emailPasswordUserProvider.GetUserByCredentialsAndRole(email, password, role,
            cancellationToken);


        if (resultUser.IsFailure)
        {
            using (_logger.BeginErrorScope(resultUser.Error))
            {
                _logger.LogWarning("Auth error. Email: {Email}", email);
            }

            return Result<AuthJwtResponseDto>.Fail(new Error(ErrorMessagePublic.AuthenticationFailed,
                ErrorType.Unauthorized));
        }


        return await CreateJwtSessionByUser(ip, fingerprint, cancellationToken, resultUser.Value);
    }

  

    public async Task<Result<AuthJwtResponseDto>> RefreshAuth(
        string refreshToken,
        IPAddress? ip, CancellationToken cancellationToken,
        string? fingerprint = null)
    {
      
        await _unitOfWork.StartTransaction(cancellationToken);
        var date = _dateTimeProvider.OffsetNow.AddSeconds(-_authOption.RefreshTokenLifetime);
        
        var userResult = await _sessionService.GetUserBySession(
            refreshToken,
            fingerprint,
            date,
            cancellationToken
        );

       
        
        if (userResult.IsFailure)
        {
            using (_logger.BeginErrorScope(userResult.Error))
            {
                _logger.LogWarning("Session was not found. RefreshToken: {RefreshToken}, Fingerprint: {Fingerprint}, Date: {Date}", 
                   refreshToken,
                    fingerprint,
                    date
                );
            }

            await _unitOfWork.RollbackTransaction(cancellationToken);
            
            if(userResult.Error.Code==(int)IdentityErrorCode.AccountBlocked)
                return Result<AuthJwtResponseDto>.Fail(new Error(ErrorMessagePublic.AccountBlocked,
                    ErrorType.Forbidden));
            
            return Result<AuthJwtResponseDto>.Fail(new Error(ErrorMessagePublic.UpdateDataSessionFailed,
                ErrorType.Unauthorized));
        }



        var removeResult = await _sessionService.RemoveSessionById(userResult.Value.Session.Id.ValueLong, cancellationToken);

        if (removeResult.IsFailure)
        {
            using (_logger.BeginErrorScope(userResult.Error))
            {
                _logger.LogError("Removing by sessionId was failed. SessionId: {SessionId}", 
                    userResult.Value.Session.Id.ValueLong);
            }

            await _unitOfWork.RollbackTransaction(cancellationToken);
            return Result<AuthJwtResponseDto>.Fail(new Error(ErrorMessagePublic.UpdateDataSessionFailed,
                ErrorType.Unauthorized));
        }
        
        var resultDto  =  await CreateJwtSessionByUser(ip, fingerprint, cancellationToken, userResult.Value.User);
        
        await _unitOfWork.CommitTransaction(cancellationToken);
        
        
        
        return resultDto;
    }
    
    private async Task<Result<AuthJwtResponseDto>> CreateJwtSessionByUser(IPAddress? ip, string? fingerprint, CancellationToken cancellationToken,
        User user)
    {
        var dateCreated = _dateTimeProvider.OffsetNow;
        var dateExpiresRefresh = dateCreated.AddSeconds(_authOption.RefreshTokenLifetime);
        var claims = _claimProvider.GetClaims(user);
        var accessToken = _cryptoIdentityService.GenerateAccessToken(claims, dateExpiresRefresh.UtcDateTime.Date);
        var refreshToken = _cryptoIdentityService.GenerateRefreshToken();

       
        var session = new Session(accessToken: accessToken,
            refreshToken: refreshToken,
            userId: user.Id.ValueLong,
            createdAt: new AppDate(dateCreated),
            refreshTokenExpiresAt: new AppDate(dateExpiresRefresh),
            fingerprint: fingerprint,
            ip: ip,
            authProvider: AuthProvider.Email
        );

      
        var sessionResult = await _sessionService.Create(session, cancellationToken);

        if (sessionResult.IsFailure)
        {
            using (_logger.BeginErrorScope(sessionResult.Error))
            {
                _logger.LogWarning("Session crate error. Session: {Session}", session);
            }

            return Result<AuthJwtResponseDto>.Fail(new Error(ErrorMessagePublic.AuthenticationFailed,
                ErrorType.Unauthorized));
        }
        
        var result = new AuthJwtResponseDto(accessToken, refreshToken, _authOption.AccessTokenLifetime,
            new AuthUserResponseDto(user.Id.ValueGuid,
                user.Name.Value,
                user.Contact
            )
        );

        return Result<AuthJwtResponseDto>.Ok(result);
    }


    public async Task<User?> GetUserByGuidId(Guid guid, CancellationToken cancellationToken)
        => await _userRepository.GetByGuidId(guid, cancellationToken);


    public async Task<User> GetUserByDto(
        LoginEmailRoleFingerprintRequestDto dto,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();


        // var role = await _roleRepository.GetByAlias(dto.Role, cancellationToken);
        // if (role == null)
        //     throw new AuthenticationException(IdentityErrorConstants.AuthenticationWasFailedStep0);
        // var user = await _userRepository.GetByEmail(dto.Email, cancellationToken);
        // if (user == null) throw new AuthenticationException(IdentityErrorConstants.AuthenticationWasFailedStep1);
        // if (user.Roles.All(p => p.Alias != dto.Role))
        //     throw new AuthenticationException(IdentityErrorConstants.AuthenticationWasFailedStep2);
        //
        //
        //
        //
        //
        // var salt = _cryptoService.GetSaltBytes(user.PasswordSalt);
        // var hash = _cryptoService.GetHashPassword(dto.Password, salt);
        //
        // var validUser =
        //     await _userRepository.GetByEmailAndHashPassword(dto.Email, hash, cancellationToken);
        //
        // if (validUser == null)
        //     throw new CommonApplicationException(IdentityErrorConstants.AuthenticationWasFailedStep3);
        //
        //
        // return validUser;
    }


    public AuthJwtResponseDto GenerateAuthDto(User user)
    {
        throw new NotImplementedException();


        //    if (user == null)
        //        throw new ArgumentNullException(nameof(user));
        //    if (user.Roles is not { })
        //        throw new InvalidOperationException($"User.Roles is null! User: {user.GuidId}");
        //
        //    if (user.Roles.Count == 0)
        //        throw new InvalidOperationException($"User.Roles is empty! User: {user.GuidId}");
        //    var claims = CreateClaims(user);
        //
        //    var identityContact = user.Email ?? user.Phone;
        //    claims.Add(new Claim(IdentityAuthConstants.NameClaim, identityContact));
        //    claims.Add(new Claim(IdentityAuthConstants.UserIdClaim, user.GuidId.ToString()));
        //
        //    var accessToken = _cryptoService.GenerateAccessToken(claims);
        //    var refreshToken = _cryptoService.GenerateRefreshToken();
        //
        //    // var result = new AuthJwtResponseDto(accessToken,
        //    //     refreshToken,
        //    //     _authOption.AccessTokenLifetime,
        //    //     new AuthUserResponseDto(user.GuidId,
        //    //         user.Name ?? String.Empty,
        //    //         user.Roles.Select(p => p.Alias).ToArray(),
        //    //         user.Email,
        //    //         user.Phone)
        //    // );
        // //   userAuthModel.Roles.AddRange(user.Roles.Select(p => p.Alias).ToArray());
        //
        //    var userAuthModel = new AuthUserResponseDto(
        //        user.GuidId.ToString(),
        //        user.Name ?? String.Empty,
        //        user.Roles.Select(p => p.Alias).ToList(),
        //        user.Email ?? String.Empty,
        //        user.Phone ?? String.Empty
        //        );
        //        
        //        
        //        
        //
        //    var result = new AuthJwtResponseDto(
        //        accessToken,
        //        refreshToken,
        //        _authOption.AccessTokenLifetime,
        //        userAuthModel
        //    );
        //    
        //
        //    return result;
    }

    public List<Claim> CreateClaims(User user)
    {
        throw new NotImplementedException();
        // var claims = // Enumerable.Empty<Claim>();
        //     user.Roles.SelectMany(p => new[]
        //     {
        //         new Claim(ClaimTypes.NameIdentifier, user.GuidId.ToString()),
        //         new Claim(ClaimTypes.Role, p.Alias),
        //         new Claim(IdentityAuthConstants.RoleClaim, p.Alias)
        //     }).ToList();
        // return claims;
    }
}