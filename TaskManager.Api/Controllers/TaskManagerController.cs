using Microsoft.AspNetCore.Mvc;
using TaskManager.Domain.DTOs;
using TaskManager.Domain.Services;
using Microsoft.AspNetCore.Authorization;

namespace TaskManager.API.Controllers;

[Authorize]
[Route("api/tasks")]
[ApiController]
public class TasksController : ControllerBase
{
    private readonly TaskService _taskService;

    public TasksController(TaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto createTaskDto)
    {
        await _taskService.CreateTaskAsync(createTaskDto);
        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDetailsDto>> GetTaskDetails(int id)
    {
        var task = await _taskService.GetTaskDetailsAsync(id);
        return Ok(task);
    }

    [HttpGet]
    public async Task<ActionResult<List<TaskListDto>>> GetTasks()
    {
        var tasks = await _taskService.GetTasksAsync();
        return Ok(tasks);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTaskAttributes(int id, [FromBody] UpdateTaskDto updateTaskDto)
    {
        await _taskService.UpdateTaskAttributesAsync(id, updateTaskDto);
        return Ok();
    }

    [HttpPut("{id}/assignee")]
    public async Task<IActionResult> UpdateTaskAssignee(int id, [FromBody] UpdateTaskAssigneeDto updateAssigneeDto)
    {
        await _taskService.UpdateTaskAssigneeAsync(id, updateAssigneeDto);
        return Ok();
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] UpdateTaskStatusDto updateStatusDto)
    {
        await _taskService.UpdateTaskStatusAsync(id, updateStatusDto);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        await _taskService.DeleteTaskAsync(id);
        return NoContent();
    }
}