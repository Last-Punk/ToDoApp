using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Data.Models;

public class TaskStatus
{
    public enum Statuses
    {
        ToDo, InProgress, InReview, Done, Paused, Failed
    }
    [Required]
    [Key]
    public int TaskStatusId { get; set; }
    [Required]
    public Statuses Status { get; set; }
    [NotMapped]
    public string DisplayStatus => Status switch
        {
            Statuses.ToDo => "To Do",
            Statuses.InProgress => "In Progress",
            Statuses.InReview => "In Review",
            Statuses.Done => "Done",
            Statuses.Paused => "Paused",
            Statuses.Failed => "Failed",
            _ => throw new ArgumentException($"Invalid status value: {Status}")
        };
}
