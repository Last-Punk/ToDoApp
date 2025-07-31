using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Data.Models;

public class Task
{
    [Required]
    [Key]
    public int TaskId { get; set; }
    [Required]
    [StringLength(50)]
    public required string Description { get; set; }
    public DateTime? DueDate { get; set; }
    [Required]
    public int TaskStatusId { get; set; }
    [StringLength(20)]
    public string? UserId { get; set; }
    [ForeignKey("UserId")]
    public User? User { get; set; }
    [ForeignKey("TaskStatusId")]
    public TaskStatus? TaskStatus { get; set; }
}
