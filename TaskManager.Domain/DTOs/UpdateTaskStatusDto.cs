namespace TaskManager.Domain.DTOs;

public class UpdateTaskStatusDto
{
    public Data.Models.TaskStatus.Statuses NewStatus { get; set; }
}