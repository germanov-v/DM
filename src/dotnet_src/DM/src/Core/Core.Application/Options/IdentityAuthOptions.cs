using System.ComponentModel.DataAnnotations;

namespace Core.Application.Options;

public class IdentityAuthOptions
{
    /// <summary>
    /// in seconds
    /// </summary>
    public int AccessTokenLifetime { get; set; }
    
    
    /// <summary>
    /// in seconds
    /// </summary>
    public int RefreshTokenLifetime { get; set; }

    public required string Url { get; set; }

    [Required]
    public required string CryptoKey { get; set; } 
}