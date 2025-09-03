namespace Core.Application.Dto.Identity;

public record AuthJwtResponseDto(string AccessToken, 
    string RefreshToken, 
    int ExpiresIn, 
    AuthUserResponseDto User);

public record AuthJwtResponse(string AccessToken, 
    int ExpiresIn, 
    AuthUserResponseDto User);

public record AuthUserResponseDto(
    string GuidId,
    string Name,
    List<string> Roles,
    string Contact);