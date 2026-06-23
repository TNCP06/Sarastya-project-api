using System.Globalization;

namespace SaraDrive.Infrastructure.Data;

// Produces the exact text format the schema's now_text() emits: 'YYYY-MM-DD HH:MM:SS' (UTC).
// Used by write repos for updated_at/deleted_at so values round-trip identically with the
// Python engine and the web's date parsing.
public static class SqlTime
{
    public static string NowText()
        => DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}
