using FluentValidation;

namespace Core.Application.Handlers.Identity.Dto.Validators;

public class LoginEmailValidator : AbstractValidator<LoginEmailRoleFingerprintRequestDto>
{
    public LoginEmailValidator()
    {
        RuleFor(p => p.Email)
            .NotEmpty()
            .WithMessage("Name is required")
            .EmailAddress();
        
        
        // RuleFor(p => p.Role)
        //     .Empty().WithMessage("Name is required")
        //    ;
        
        // RuleFor(p => p.Fingerprint)
        //     .NotEmpty().WithMessage("Name is required")
        //     ;
        
        RuleFor(p => p.Password)
            .NotEmpty().WithMessage("Введите пароль")
            ;
        
        // RuleFor(p => p.Fingerprint)
        //     .NotEmpty().WithMessage("Отпечаток пальца обязателен для заполнения");
    }
}