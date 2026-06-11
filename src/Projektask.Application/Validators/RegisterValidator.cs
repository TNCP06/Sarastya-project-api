using FluentValidation;
using Projektask.Application.DTOs;

namespace Projektask.Application.Validators;

public class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nama wajib diisi")
            .Length(2, 100).WithMessage("Nama harus antara 2–100 karakter");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email wajib diisi")
            .EmailAddress().WithMessage("Format email tidak valid");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password wajib diisi")
            .MinimumLength(8).WithMessage("Password minimal 8 karakter");
    }
}
