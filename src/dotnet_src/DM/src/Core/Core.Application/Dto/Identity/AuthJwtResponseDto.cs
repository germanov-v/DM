namespace Core.Application.Dto.Identity;

public record AuthJwtResponseDto(string AccessToken, 
    string RefreshToken, 
    int ExpiresIn, 
    AuthUserResponseDto User);

public record AuthJwtResponse(string AccessToken, 
    int ExpiresIn, 
    AuthUserResponseDto User);

public record AuthUserResponseDto(
    Guid GuidId,
    string Name,
    string Contact);