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
using Core.Application.Options.Identity;
using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.BoundedContext.Identity.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Core.Application.Handlers.Identity;



public class IdentityHandler : IIdentityHandler
{

    private readonly ICryptoIdentityService _cryptoIdentityService;
  private readonly IEmailPasswordProvider _emailPasswordProvider;
    private readonly ISessionRepository _authTokenRepository;
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
        IEmailPasswordProvider emailPasswordProvider, ILogger<IdentityHandler> logger, IClaimProvider claimProvider, IDateTimeProvider dateTimeProvider)
    {
        _authOption = authOption.Value;
        _authTokenRepository = authTokenRepository;
        _cryptoIdentityService = cryptoIdentityService;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _emailPasswordProvider = emailPasswordProvider;
        _logger = logger;
        _claimProvider = claimProvider;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<AuthJwtResponseDto>> AuthenticateByEmailPasswordRole(string email,
        string password, 
        string role,
        IPAddress? ip,
        string fingerprint,
        CancellationToken cancellationToken)
    {
        
        var resultUser = await _emailPasswordProvider.GetUserByCredential(email, password, cancellationToken);

        
        if (resultUser.IsFailure)
        {
            Result<AuthJwtResponseDto>.Fail(resultUser.Error with { Type = ErrorType.Forbidden });
            return Result<AuthJwtResponseDto>.Fail(resultUser.Error with { Type = ErrorType.Forbidden });
        }
        
        var resultRole = await _emailPasswordProvider.IsInRole(resultUser.Value, role, cancellationToken);

        if (!resultRole)
        {
            return Result<AuthJwtResponseDto>.Fail(new Error("User is not in role", ErrorType.Forbidden));
        }


        
        
        var dateCreated = _dateTimeProvider.OffsetNow;
        var dateExpiresRefresh = dateCreated.AddSeconds(_authOption.RefreshTokenLifetime);
        var claims = _claimProvider.GetClaims(resultUser.Value);
        var accessToken = _cryptoIdentityService.GenerateAccessToken(claims, dateExpiresRefresh.DateTime);
        var refreshToken = _cryptoIdentityService.GenerateRefreshToken();


        var entity = new Session(accessToken: accessToken,
             refreshToken: refreshToken,
             userId: resultUser.Value.Id.ValueLong,
             dateCreated: dateCreated,
             refreshExpired: dateExpiresRefresh,
             fingerprint: fingerprint,
             ip: ip
        );
       
        // await _authTokenRepository.Create(entity, cancellationToken);
       // var session = await _authTokenRepository.Create(entity, cancellationToken);
        _ = await _authTokenRepository.Create(entity, cancellationToken);
        
        var result = new AuthJwtResponseDto(accessToken, refreshToken, _authOption.AccessTokenLifetime,
            new AuthUserResponseDto(resultUser.Value.Id.ValueGuid,  
                resultUser.Value.Name.Value,
                resultUser.Value.Contact
                )
            );
        
        return  Result<AuthJwtResponseDto>.Ok(result);
    }


    public async Task<AuthJwtResponseDto> RefreshAuth(RefreshTokenDto dto, CancellationToken cancellationToken,
        string? refreshToken = null)
    {
        throw new NotImplementedException();
        // await _unitOfWork.StartTransaction(cancellationToken);
        // var date = DateTimeApplication.GetCurrentDate().AddSeconds(-_authOption.RefreshTokenLifetime);
        // var authToken = await _authTokenRepository
        //     .GetByRefreshTokenFingerprint(
        //         dto.RefreshToken,
        //         dto.Fingerprint,
        //         date,
        //         cancellationToken
        //     );
        //
        //
        // if (authToken == null)
        //     throw new ForbiddenApplicationException("Refresh auth was failed! Token not found!",
        //         new Dictionary<string, string>()
        //         {
        //             { "RefreshToken", dto.RefreshToken },
        //             { "Fingerprint", dto.Fingerprint },
        //             { "date", date.ToString(CultureInfo.InvariantCulture) },
        //         });
        //
        // var user = await _userRepository.GetById(authToken.UserId, cancellationToken);
        // if (user == null) throw new InvalidOperationException("Refresh auth was failed! User not found!");
        //
        //
        // var result = GenerateAuthDto(user);
        //
        // var entity = new AuthToken()
        // {
        //     AccessToken = result.AccessToken,
        //     CreatedDate = date,
        //     RefreshToken = refreshToken ?? result.RefreshToken,
        //     UserId = user.Id,
        //     RefreshTokenDateExpired = date.AddSeconds(_authOption.RefreshTokenLifetime),
        //     Fingerprint = dto.Fingerprint
        // };
        //
        // //    await using var transaction = await _unitOfWork.CreateTransaction(cancellationToken, IsolationLevelConstants.IdentityBaseIsolationLevel);
        //
        //
        // // await _authTokenRepository.Remove(authToken, cancellationToken, _unitOfWork.CurrentTransaction);
        // // await _authTokenRepository.Create(entity, cancellationToken, _unitOfWork.CurrentTransaction);
        //
        //
        //
        // await _authTokenRepository.Remove(authToken, cancellationToken);
        // await _authTokenRepository.Create(entity, cancellationToken);
        //
        // await _unitOfWork.CommitTransaction(cancellationToken);
        //
        //
        //
        // return result;
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


