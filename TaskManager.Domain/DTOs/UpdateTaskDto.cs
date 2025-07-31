namespace TaskManager.Domain.DTOs;

public class UpdateTaskDto
{
    public required string Description { get; set; }
    public DateTime? DueDate { get; set; }
}