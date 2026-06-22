namespace SaraDrive.Domain.Entities;

// Web/mobile account for JWT auth. Standalone — the drive itself is single-tenant
// (one owner's library), so there is no per-user partitioning of folders/items; a valid
// token simply grants access to the drive. See README "Authorization model".
public class User
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // TEXT 'YYYY-MM-DD HH:MM:SS' (UTC) — DB default now_text(); see schema convention.
    public string CreatedAt { get; set; } = string.Empty;
}
