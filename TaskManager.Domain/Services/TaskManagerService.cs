using TaskManager.Domain.DTOs;
using TaskManager.Data;
using TaskManager.Data.Models;
using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Exceptions;

namespace TaskManager.Domain.Services;

public class TaskService(Database context, ICurrentUserService currentUserService)
{
    public async System.Threading.Tasks.Task CreateTaskAsync(CreateTaskDto createTaskDto)
    {
        var newTaskStatus = new Data.Models.TaskStatus
        {
            Status = Data.Models.TaskStatus.Statuses.ToDo
        };

        await context.Statuses.AddAsync(newTaskStatus);
        await context.SaveChangesAsync();

        var task = new Data.Models.Task
        {
            Description = createTaskDto.Description,
            DueDate = createTaskDto.DueDate,
            TaskStatusId = newTaskStatus.TaskStatusId,
            UserId = currentUserService.GetUserId()
        };

        await context.Tasks.AddAsync(task);
        await context.SaveChangesAsync();
    }
    public async Task<TaskDetailsDto> GetTaskDetailsAsync(int taskId)
    {
        var task = await context.Tasks
            .Include(t => t.TaskStatus)
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TaskId == taskId)
            ?? throw new TaskNotFoundException();

        var currentUserId = currentUserService.GetUserId();

        if (task.UserId != currentUserId)
        {
            throw new UserDoesNotOwnTaskException();
        }

        return new TaskDetailsDto
        {
            TaskId = task.TaskId,
            Description = task.Description,
            DueDate = task.DueDate,
            Status = task.TaskStatus?.DisplayStatus ?? "N/A",
            AssignedToUser = task.User?.UserName ?? "None"
        };
    }
    public async Task<List<TaskListDto>> GetTasksAsync()
    {
        var userId = currentUserService.GetUserId();

        return await context.Tasks
            .Where(t => t.User!.Id == userId)
            .Include(t => t.TaskStatus)
            .Include(t => t.User)
            .Select(t => new TaskListDto
            {
                TaskId = t.TaskId,
                Description = t.Description,
                DueDate = t.DueDate,
                Status = t.TaskStatus!.DisplayStatus,
                AssignedToUser = t.User!.UserName
            })
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task UpdateTaskAttributesAsync(int taskId, UpdateTaskDto updateTaskDto)
    {
        var task = await context.Tasks.FindAsync(taskId)
            ?? throw new TaskNotFoundException();

        if (task.UserId != currentUserService.GetUserId())
        {
            throw new UserDoesNotOwnTaskException();
        }

        task.Description = updateTaskDto.Description;
        task.DueDate = updateTaskDto.DueDate;
        await context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task UpdateTaskAssigneeAsync(int taskId, UpdateTaskAssigneeDto updateAssigneeDto)
    {
        var task = await context.Tasks.FindAsync(taskId)
            ?? throw new TaskNotFoundException();

        var user = await context.Users.FindAsync(updateAssigneeDto.UserId)
            ?? throw new UserNotFoundException("Assigned user not found.");

        if (task.UserId != currentUserService.GetUserId())
        {
            throw new UserDoesNotOwnTaskException();
        }

        task.UserId = updateAssigneeDto.UserId;
        await context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task UpdateTaskStatusAsync(int taskId, UpdateTaskStatusDto updateStatusDto)
    {
        var task = await context.Tasks.FindAsync(taskId)
            ?? throw new TaskNotFoundException();

        if (task.UserId != currentUserService.GetUserId())
        {
            throw new UserDoesNotOwnTaskException();
        }

        if (task.TaskStatus != null && task.TaskStatus.Status == updateStatusDto.NewStatus)
        {
            throw new TaskAlreadyHasThisStatusException($"Task already has status '{updateStatusDto.NewStatus}'.");
        }

        var newStatusEntry = new Data.Models.TaskStatus
        {
            Status = updateStatusDto.NewStatus
        };

        await context.Statuses.AddAsync(newStatusEntry);
        await context.SaveChangesAsync();

        task.TaskStatusId = newStatusEntry.TaskStatusId;
        await context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task DeleteTaskAsync(int taskId)
    {
        var task = await context.Tasks.FindAsync(taskId)
            ?? throw new TaskNotFoundException();

        if (task.UserId != currentUserService.GetUserId())
        {
            throw new UserDoesNotOwnTaskException();
        }

        context.Tasks.Remove(task);
        await context.SaveChangesAsync();
    }
}