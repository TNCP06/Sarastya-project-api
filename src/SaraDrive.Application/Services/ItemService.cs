using SaraDrive.Application.DTOs;
using SaraDrive.Application.Exceptions;
using SaraDrive.Application.Interfaces;

namespace SaraDrive.Application.Services;

public class ItemService(IItemWriteRepository repo, IDriveReadRepository read) : IItemService
{
    private static readonly string[] ValidKinds = ["archive", "media"];

    public async Task<ItemDetailDto> UpdateAsync(long id, ItemUpdateDto dto)
    {
        var item = await repo.GetByIdAsync(id) ?? throw new NotFoundException();

        var title = dto.Title.Trim();
        if (title.Length == 0) throw new BadRequestException("Judul tidak boleh kosong");
        if (!ValidKinds.Contains(dto.Kind)) throw new BadRequestException("Kind tidak valid");

        item.Title = title;
        item.Kind = dto.Kind;            // slug intentionally unchanged (immutable grouping key)
        await repo.UpdateAsync(item);

        if (dto.Tags is not null)
            await repo.ReplaceTagsAsync(id, dto.Tags);

        return await read.GetItemAsync(id) ?? throw new NotFoundException();
    }

    public async Task SetFavoriteAsync(long id, bool value)
    {
        var item = await repo.GetByIdAsync(id) ?? throw new NotFoundException();
        item.IsFavorite = value;
        await repo.UpdateAsync(item);
    }

    public async Task SetPrivateAsync(long id, bool value)
    {
        var item = await repo.GetByIdAsync(id) ?? throw new NotFoundException();
        item.IsPrivate = value;
        await repo.UpdateAsync(item);
    }

    public async Task MoveAsync(long id, long? folderId)
    {
        var item = await repo.GetByIdAsync(id) ?? throw new NotFoundException();
        item.FolderId = folderId;
        await repo.UpdateAsync(item);
    }

    public async Task SoftDeleteAsync(long id)
    {
        _ = await repo.GetByIdAsync(id) ?? throw new NotFoundException();
        await repo.SoftDeleteAsync(id);
    }

    public async Task RestoreAsync(long id)
    {
        _ = await repo.GetByIdAsync(id) ?? throw new NotFoundException();
        await repo.RestoreAsync(id);
    }

    public async Task PurgeAsync(long id)
    {
        var item = await repo.GetByIdAsync(id) ?? throw new NotFoundException();
        if (item.DeletedAt is null)
            throw new BadRequestException("Item harus ada di Trash sebelum dihapus permanen");
        await repo.PurgeAsync(id);
    }
}
