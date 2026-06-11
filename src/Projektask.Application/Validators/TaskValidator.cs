using FluentValidation;
using Projektask.Application.DTOs;

namespace Projektask.Application.Validators;

public class TaskValidator : AbstractValidator<TaskUpsertDto>
{
    private static readonly string[] ValidStatuses = ["todo", "in_progress", "done"];

    public TaskValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Judul task wajib diisi")
            .MaximumLength(150).WithMessage("Judul task maksimal 150 karakter");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Deskripsi maksimal 1000 karakter")
            .When(x => x.Description != null);

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status wajib diisi")
            .Must(s => ValidStatuses.Contains(s))
            .WithMessage("Status harus salah satu dari: todo, in_progress, done");
    }
}
