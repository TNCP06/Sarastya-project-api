using FluentValidation;
using SaraDrive.Application.DTOs;

namespace SaraDrive.Application.Validators;

public class FolderCreateValidator : AbstractValidator<FolderCreateDto>
{
    public FolderCreateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nama folder wajib diisi")
            .MaximumLength(200).WithMessage("Nama folder maksimal 200 karakter");
    }
}

public class FolderRenameValidator : AbstractValidator<FolderRenameDto>
{
    public FolderRenameValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nama folder wajib diisi")
            .MaximumLength(200).WithMessage("Nama folder maksimal 200 karakter");
    }
}

public class ItemUpdateValidator : AbstractValidator<ItemUpdateDto>
{
    public ItemUpdateValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Judul wajib diisi")
            .MaximumLength(500).WithMessage("Judul maksimal 500 karakter");

        RuleFor(x => x.Kind)
            .Must(k => k is "archive" or "media").WithMessage("Kind harus 'archive' atau 'media'");
    }
}

public class TagUpsertValidator : AbstractValidator<TagUpsertDto>
{
    public TagUpsertValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nama tag wajib diisi")
            .MaximumLength(100).WithMessage("Nama tag maksimal 100 karakter");
    }
}

public class UploadEnqueueValidator : AbstractValidator<UploadEnqueueDto>
{
    public UploadEnqueueValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Judul wajib diisi")
            .MaximumLength(500).WithMessage("Judul maksimal 500 karakter");

        RuleFor(x => x.Kind)
            .Must(k => k is "archive" or "media").WithMessage("Kind harus 'archive' atau 'media'");

        RuleFor(x => x.SourcePath)
            .NotEmpty().WithMessage("source_path wajib diisi");
    }
}
