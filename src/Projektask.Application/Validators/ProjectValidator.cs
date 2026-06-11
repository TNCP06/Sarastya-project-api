using FluentValidation;
using Projektask.Application.DTOs;

namespace Projektask.Application.Validators;

public class ProjectValidator : AbstractValidator<ProjectUpsertDto>
{
    public ProjectValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nama project wajib diisi")
            .MaximumLength(150).WithMessage("Nama project maksimal 150 karakter");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Deskripsi maksimal 1000 karakter")
            .When(x => x.Description != null);
    }
}
