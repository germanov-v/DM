namespace Core.Application.Handlers.Identity.Dto;

public class RefreshTokenDto
{
    
    public RefreshTokenDto(string refreshToken, string fingerprint)
    {
        RefreshToken = refreshToken; //?? throw new ArgumentNullException(nameof(refreshToken));
        Fingerprint = fingerprint;
    }

    public string RefreshToken { get; set; }

    public string Fingerprint { get; set; } 
}