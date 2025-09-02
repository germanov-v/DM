namespace Core.Application.Dto.Identity;


// public class LoginEmailRequestDto
// {
//     public string Email { get; set; }
//     
//     public string Password { get; set; }
//     
//     public string Role { get; set; }
//     
//     public string Fingerprint { get; set; }
// }

public record LoginEmailFingerprintRequestDto(string Email, string Password,
    string Fingerprint);

public record LoginEmailRequestDto(string Email, string Password);


// LoginEmailRequestDto
public record LoginEmailRoleFingerprintRequestDto(
    string Email,
    string Password,
    string Role,
    string Fingerprint
);