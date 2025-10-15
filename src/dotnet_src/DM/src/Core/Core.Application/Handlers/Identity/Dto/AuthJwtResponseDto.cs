namespace Core.Application.Handlers.Identity.Dto;

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