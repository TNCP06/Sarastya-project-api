namespace SaraDrive.Application.Options;

// Public base URL of the Python streamer (e.g. https://stream.tncp.web.id), surfaced to
// clients via /api/items/{id}/stream-info so the player can build video URLs. From env Streamer__BaseUrl.
public record StreamerSettings(string BaseUrl);
