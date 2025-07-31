namespace TaskManager.Domain.DTOs;

public class CreateTaskDto
{
    public required string Description { get; set; }
    public DateTime? DueDate { get; set; }
    public string? UserId { get; set; }
}