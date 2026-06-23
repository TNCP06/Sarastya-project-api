using SaraDrive.Application.DTOs;
using SaraDrive.Application.Exceptions;
using SaraDrive.Application.Interfaces;
using SaraDrive.Domain.Entities;

namespace SaraDrive.Application.Services;

public class FolderService(IFolderWriteRepository repo) : IFolderService
{
    public async Task<FolderDto> CreateAsync(FolderCreateDto dto)
    {
        var name = dto.Name.Trim();
        if (name.Length == 0) throw new BadRequestException("Nama folder tidak boleh kosong");

        var folder = await repo.CreateAsync(new Folder
        {
            Name = name,
            ParentId = dto.ParentId,
            IsPrivate = dto.IsPrivate
        });
        return ToDto(folder);
    }

    public async Task<FolderDto> RenameAsync(long id, FolderRenameDto dto)
    {
        var name = dto.Name.Trim();
        if (name.Length == 0) throw new BadRequestException("Nama folder tidak boleh kosong");

        var folder = await repo.GetByIdAsync(id) ?? throw new NotFoundException();
        folder.Name = name;
        return ToDto(await repo.UpdateAsync(folder));
    }

    public async Task MoveAsync(long id, FolderMoveDto dto)
    {
        var folder = await repo.GetByIdAsync(id) ?? throw new NotFoundException();

        if (dto.TargetParentId is { } target)
        {
            if (target == id)
                throw new BadRequestException("Tidak bisa memindahkan folder ke dalam dirinya sendiri");
            var descendants = await repo.GetDescendantFolderIdsAsync(id); // includes self
            if (descendants.Contains(target))
                throw new BadRequestException("Tidak bisa memindahkan folder ke dalam subfolder-nya sendiri");
        }

        folder.ParentId = dto.TargetParentId;
        await repo.UpdateAsync(folder);
    }

    public async Task SetPrivateAsync(long id, bool value)
    {
        _ = await repo.GetByIdAsync(id) ?? throw new NotFoundException();
        await repo.SetPrivateRecursiveAsync(id, value);
    }

    public async Task DeleteAsync(long id)
    {
        _ = await repo.GetByIdAsync(id) ?? throw new NotFoundException();
        await repo.DeleteRecursiveAsync(id);
    }

    public async Task PurgeAsync(long id)
    {
        _ = await repo.GetByIdAsync(id) ?? throw new NotFoundException();
        await repo.PurgeAsync(id);
    }

    private static FolderDto ToDto(Folder f)
        => new(f.Id, f.Name, f.ParentId, f.IsPrivate, f.CreatedAt, f.UpdatedAt, f.DeletedAt);
}
