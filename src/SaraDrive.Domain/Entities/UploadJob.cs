namespace SaraDrive.Domain.Entities;

// A queued upload, executed by the Python watcher. The API only enqueues (origin='upload',
// pointing at a staged file) and reports status; the watcher does the Telegram I/O.
public class UploadJob
{
    public long Id { get; set; }
    public string Kind { get; set; } = "archive";          // 'archive' | 'media'
    public string Title { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;        // comma-separated
    public string SourcePath { get; set; } = string.Empty;
    public int PartSize { get; set; } = 1500;               // MB (archive only)
    public string Origin { get; set; } = "local";           // 'local' | 'upload'
    public bool CleanupSource { get; set; }
    public int PartsDone { get; set; }
    public long TotalBytes { get; set; }
    public string Status { get; set; } = "queued";          // queued|pending|running|done|error|canceled
    public int Progress { get; set; }                       // 0..100
    public string? Message { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}
