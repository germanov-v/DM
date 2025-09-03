

namespace Core.Application.Constants;

public static class IdentityAuthConstants
{
  //  public const string AuthScheme = JwtBearerDefaults.AuthenticationScheme;
    
    public const string AuthSchemeCookie = "AuthSchemeCookie";
    
    public const string AuthPolicyDefault = "AuthPolicyDefault";
    
    public const string AuthPolicyModerator = "AuthPolicyModerator";
    
    public const string AuthPolicyCandidate = "AuthPolicyCandidate";
    
    public const string AuthPolicyCompany = "AuthPolicyCompany";
    
    public const string CorsPolicyName = "CorsPolicy";
    
    public const string RoleClaim = "Roles";
    
    public const string NameClaim = "Name";
    
    public const string UserIdClaim = "UserId";
}