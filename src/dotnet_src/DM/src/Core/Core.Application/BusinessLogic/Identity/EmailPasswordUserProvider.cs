using Core.Application.Abstractions.BusinessLogic.Identity;
using Core.Application.Abstractions.Services;
using Core.Application.Abstractions.Services.Identity;
using Core.Application.Common.Results;
using Core.Application.Handlers.Identity;
using Core.Application.Handlers.Identity.Errors;
using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.BoundedContext.Identity.Repositories;
using Core.Domain.BoundedContext.Identity.ValueObjects;
using Core.Domain.SharedKernel.Results;
using Core.Domain.SharedKernel.ValueObjects;

namespace Core.Application.BusinessLogic.Identity;

public class EmailPasswordUserProvider : IEmailPasswordUserProvider
{
    
    private readonly IUserRepository _userRepository;
    private readonly ICryptoIdentityService _cryptoService;
    private readonly TimeProvider _dateTimeProvider;

    public EmailPasswordUserProvider(IUserRepository userRepository, ICryptoIdentityService cryptoService, 
        TimeProvider dateTimeProvider)
    {
        _userRepository = userRepository;
        _cryptoService = cryptoService;
        _dateTimeProvider = dateTimeProvider;
    }

   
    
    public async Task<Result<IdGuid>> Create(string email, string password, string name, string[] roleAliases, 
        CancellationToken cancellationToken
        , bool isActive = false)
    {
        
        // var salt = _cryptoService.CreateSalt();
        // var hashPassword = _cryptoService.GetHashPassword(password,salt);
        // var saltPassword = _cryptoService.GetSaltStr(salt);

        var (hashPassword, saltPassword, salt) = GetHashPassword(password);
        
        var guidId = IdGuid.New();
        var date = _dateTimeProvider.GetLocalNow();
        var user = new User(
            new EmailIdentity(email), 
            confirmed: new Status(isActive, date),
            new BlockStatus(false,date, null, null),
            new Name(name),
            id: guidId,
            password: new Password(hashPassword, saltPassword),
            createdAt:  new AppDate(date)
        );

         
        var id = await _userRepository.Create(user,roleAliases, cancellationToken);
        
        
        return Result<IdGuid>.Ok(id);
    }

   

    public async Task<Result<User>> GetUserByCredentialsAndRole(string email, string password, string roleAlias, CancellationToken cancellationToken)
    {
     
        var user = await _userRepository.GetEmailCredentialsUserByEmail(email,  cancellationToken);
        
        if (user == null)
            return Result<User>.Fail(new Error("Credentials data is not valid or not found", ErrorType.Unauthorized, (int)IdentityErrorCode.EmailNotFound));
        
        if (user.Password == null)
            return Result<User>.Fail(new Error("Credentials data was not loaded", ErrorType.Failure, (int)IdentityErrorCode.PasswordNotFound));

        if (user.IsBlocked)
            return Result<User>.Fail(new Error($"Account was blocked: {user.Blocked.Reason}. Code: {user.Blocked.Code}", ErrorType.Forbidden, (int)IdentityErrorCode.AccountBlocked));
        
        var saltByte = _cryptoService.GetSaltBytes(user.Password.PasswordSalt);
        var  hashPassword = _cryptoService.GetHashPassword(password, saltByte);
        
        if(user.Password.PasswordHash!=hashPassword)
            return Result<User>.Fail(new Error("Credentials data is not valid", ErrorType.Unauthorized, (int)IdentityErrorCode.PasswordNotCorrect));
        
        
        
        if (!user.Confirmed.Value)
            return Result<User>.Fail(new Error("Account is not confirmed", ErrorType.Forbidden, (int)IdentityErrorCode.AccountNotConfirmed));
        
        if(!user.HasRole(roleAlias))
            return Result<User>.Fail(new Error("Insufficient permissions", ErrorType.Forbidden, (int)IdentityErrorCode.RoleNotFound));
        
        return Result<User>.Ok(user);
    }

    public Task<DomainResult<User>> GetUserByCredential(string email, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> IsInRole(User user, string role, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private (string hash, string salt, byte[] saltBytes) GetHashPassword(string password)
    {
        var salt = _cryptoService.CreateSalt();
        var hashPassword = _cryptoService.GetHashPassword(password,salt);
        var saltPassword = _cryptoService.GetSaltStr(salt);
        return (hashPassword, saltPassword, salt);
    }
}