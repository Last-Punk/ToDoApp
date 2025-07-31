namespace TaskManager.Domain.DTOs;

public class TaskListDto
{
    public int TaskId { get; set; }
    public string Description { get; set; }
    public DateTime? DueDate { get; set; }
    public string Status { get; set; }
    public string? AssignedToUser { get; set; }
}