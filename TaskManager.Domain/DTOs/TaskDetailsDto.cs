using TaskManager.Data.Models;

namespace TaskManager.Domain.DTOs;

public class TaskDetailsDto
{
    public int TaskId { get; set; }
    public string Description { get; set; }
    public DateTime? DueDate { get; set; }
    public string Status { get; set; }
    public string? AssignedToUser { get; set; }
}