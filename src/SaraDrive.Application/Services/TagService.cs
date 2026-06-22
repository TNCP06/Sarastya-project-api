using SaraDrive.Application.DTOs;
using SaraDrive.Application.Exceptions;
using SaraDrive.Application.Interfaces;

namespace SaraDrive.Application.Services;

public class TagService(IDriveReadRepository read, ITagWriteRepository write) : ITagService
{
    public Task<IEnumerable<TagDto>> GetAllAsync() => read.GetAllTagsAsync();

    public async Task<TagDto> CreateAsync(TagUpsertDto dto)
    {
        var name = dto.Name.Trim();
        if (name.Length == 0) throw new BadRequestException("Nama tag tidak boleh kosong");
        if (await write.GetByNameAsync(name) is not null)
            throw new ConflictException("Tag dengan nama itu sudah ada");

        var tag = await write.CreateAsync(name, (dto.Color ?? string.Empty).Trim());
        return new TagDto(tag.Id, tag.Name, tag.Color);
    }

    public async Task<TagDto> UpdateAsync(long id, TagUpsertDto dto)
    {
        var name = dto.Name.Trim();
        if (name.Length == 0) throw new BadRequestException("Nama tag tidak boleh kosong");

        var tag = await write.GetByIdAsync(id) ?? throw new NotFoundException();

        var clash = await write.GetByNameAsync(name);
        if (clash is not null && clash.Id != id)
            throw new ConflictException("Tag dengan nama itu sudah ada");

        tag.Name = name;
        tag.Color = (dto.Color ?? string.Empty).Trim();
        var updated = await write.UpdateAsync(tag);
        return new TagDto(updated.Id, updated.Name, updated.Color);
    }

    public async Task DeleteAsync(long id)
    {
        if (!await write.DeleteAsync(id)) throw new NotFoundException();
    }
}
