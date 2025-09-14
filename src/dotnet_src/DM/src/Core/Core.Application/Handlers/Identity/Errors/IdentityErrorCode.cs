namespace Core.Application.Handlers.Identity.Errors;

/// <summary>
/// 300X
/// </summary>
public enum IdentityErrorCode
{
    EmailNotFound = 3001,
    PasswordNotFound = 3002,
    PasswordNotCorrect = 3003,
    AccountBlockedOrNotConfirmed = 3004, // должен быть бан
    AccountNotConfirmed = 3004,
    RoleNotFound = 3005,
    SessionCreateFailed = 3006,
    SessionNotFound = 3007,
    SessionNotRemoved = 3008,
    UserNotFoundById = 3009,
    
    RefreshTokenExpired = 3010,

    AccountBlocked = 3500,
  //  BlockedReasonSpam = 3501,
}