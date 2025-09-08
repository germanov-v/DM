using Core.Application.Abstractions.BusinessLogic.Identity;
using Core.Application.Abstractions.Services.Identity;
using Core.Application.Common.Results;
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

    public EmailPasswordUserProvider(IUserRepository userRepository, ICryptoIdentityService cryptoService)
    {
        _userRepository = userRepository;
        _cryptoService = cryptoService;
    }

    public async Task<Result<IdGuid>> Create(string email, string password, string name, IEnumerable<Role> roles, CancellationToken cancellationToken
    , bool isActive = false)
    {
        
        throw new NotImplementedException();
       var hashPassword = "";
        var saltPassword = "";
        
        
        var guidId = IdGuid.New();
        var user = new User(new EmailIdentity(email), 
            new Password(hashPassword, saltPassword),
            isActive,
            new Name(name),
            roles,
            guidId
            );

         
      //  var result = await _userRepository.Create(user, cancellationToken);
    }
    
    public async Task<Result<IdGuid>> Create(string email, string password, string name, string[] roleAliases, 
        CancellationToken cancellationToken
        , bool isActive = false)
    {
        
        var salt = _cryptoService.CreateSalt();
        var hashPassword = _cryptoService.GetHashPassword(password,salt);
        var saltPassword = _cryptoService.GetSaltStr(salt);
        
        
        var guidId = IdGuid.New();
        var user = new User(new EmailIdentity(email), 
            new Password(hashPassword, saltPassword),
            isActive,
            new Name(name),
            null,
            guidId
        );

         
        var id = await _userRepository.Create(user,roleAliases, cancellationToken);
        
        
        return Result<IdGuid>.Ok(id);
    }

   

    public async Task<Result<User>> GetUserByCredential(string email, string password, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<DomainResult<User>> GetUserByCredential(string email, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> IsInRole(User user, string role, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}