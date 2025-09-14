using Core.Application.Abstractions.BusinessLogic.Identity;
using Core.Application.Common.Results;
using Core.Application.Handlers.Identity.Errors;
using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.BoundedContext.Identity.Repositories;

namespace Core.Application.BusinessLogic.Identity;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IUserRepository _userRepository;

    public SessionService(ISessionRepository sessionRepository, IUserRepository userRepository)
    {
        _sessionRepository = sessionRepository;
        _userRepository = userRepository;
    }


    public async Task<Result> Create(Session session, CancellationToken cancellationToken)
    {
        var result = await _sessionRepository.Create(session, cancellationToken);
        
        if(result.ValueLong<=0)
            return Result.Fail(new Error("Create session failed in db", ErrorType.Failure, (int)IdentityErrorCode.SessionCreateFailed));
        
        return Result.Ok();
    }

    public async Task<Result<Session>> GetValidSession(string refreshToken, string? fingerPrint, DateTimeOffset date, CancellationToken cancellationToken)
    {
        var result = await _sessionRepository
            .GetByRefreshTokenFingerprint(
                refreshToken,
                fingerPrint,
                date,
                cancellationToken
            );
        
        if(result is null)
            return Result<Session>.Fail(new Error("Session was not found", ErrorType.Failure, (int)IdentityErrorCode.SessionNotFound));

        if (result.RefreshTokenExpiresAt.Value < date)
        {
            return Result<Session>.Fail(new Error("Current session is not valid. Refresh token was expired", ErrorType.Failure, (int)IdentityErrorCode.RefreshTokenExpired));

        }
        
        return Result<Session>.Ok(result);
    }
    
    public async Task<Result> RemoveSessionById(long id, CancellationToken cancellationToken)
    {
        var result = await _sessionRepository
            .RemoveById(id,
                cancellationToken
            );
        
        if(result!=1)
            return Result<Session>.Fail(new Error("Old session was not removed", ErrorType.Failure, (int)IdentityErrorCode.SessionNotRemoved));
        
        return Result.Ok();
    }
    
    
    public async Task<Result<(User User, Session Session)>> GetUserBySession(string refreshToken, string? fingerPrint, DateTimeOffset date, CancellationToken cancellationToken)
    {
        var sessionResult = await GetValidSession(refreshToken, fingerPrint, date, cancellationToken);

        if (sessionResult.IsFailure)
            return Result<(User,Session)>.Fail(sessionResult.Error);
        
        
        
        var user = await _userRepository
            .GetById(sessionResult.Value.UserId,
                cancellationToken
            );
        
        if(user is null)
            return Result<(User,Session)>.Fail(new Error("User was not found by active user", ErrorType.Failure, (int)IdentityErrorCode.UserNotFoundById));
        
        if(!user.IsActive)
            return Result<(User,Session)>.Fail(new Error("User found but not active", ErrorType.Failure, (int)IdentityErrorCode.AccountBlockedOrNotConfirmed));

        
        
        return Result<(User,Session)>.Ok((user, sessionResult.Value));
    }
}