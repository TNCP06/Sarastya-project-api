using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaraDrive.Application.Interfaces;

namespace SaraDrive.Api.Controllers;

// Drive READ endpoints (Dapper-backed). The drive is single-tenant: a valid token grants
// access; there is no per-user filtering. `space` = main | private.
[ApiController]
[Authorize]
[Route("api")]
public class DriveController(IDriveService drive) : ControllerBase
{
    [HttpGet("drive")]
    public async Task<IActionResult> GetDrive([FromQuery] string space = "main")
        => Ok(await drive.GetDriveAsync(space));

    [HttpGet("items/{id:long}")]
    public async Task<IActionResult> GetItem(long id)
        => Ok(await drive.GetItemAsync(id));

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q = "", [FromQuery] string space = "main")
        => Ok(await drive.SearchAsync(q, space));

    [HttpGet("gallery/{id:long}")]
    public async Task<IActionResult> Gallery(long id)
        => Ok(await drive.GetGalleryAsync(id));

    [HttpGet("trash")]
    public async Task<IActionResult> Trash()
        => Ok(await drive.GetTrashAsync());

    [HttpGet("items/{id:long}/stream-info")]
    public async Task<IActionResult> StreamInfo(long id)
        => Ok(await drive.GetStreamInfoAsync(id));

    [HttpGet("parts/{id:long}/subtitles")]
    public async Task<IActionResult> Subtitles(long id)
        => Ok(await drive.GetSubtitlesAsync(id));
}
