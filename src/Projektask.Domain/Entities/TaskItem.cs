namespace Projektask.Domain.Entities;

// Nama TaskItem, bukan Task, supaya tidak bentrok dengan System.Threading.Tasks.Task
public class TaskItem
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "todo";
    public DateOnly? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }

    public Project Project { get; set; } = null!;
}
