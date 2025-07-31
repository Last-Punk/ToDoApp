using Moq;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Data.Models;
using TaskManager.Domain.DTOs;
using TaskManager.Domain.Services;
using TaskManager.Domain.Exceptions;

namespace TaskManager.Tests;

public class UpdateTaskAssigneeTests
{
    private Database GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<Database>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new Database(options);
    }

    [Fact]
    public async System.Threading.Tasks.Task ShouldUpdateTaskAssignee_WhenTaskExistsAndUserOwnsItAndAssigneeExists()
    {
        var context = GetInMemoryDbContext();
        var currentUserService = new Mock<ICurrentUserService>();
        var service = new TaskService(context, currentUserService.Object);

        var ownerId = Guid.NewGuid().ToString();
        var ownerUserName = "Owner";
        var normalizedOwnerUserName = ownerUserName.ToUpper();

        var newAssigneeId = Guid.NewGuid().ToString();
        var newAssigneeUserName = "New Assignee";
        var normalizedNewAssigneeUserName = newAssigneeUserName.ToUpper();
        var taskId = 1;

        context.Users.Add(new User { Id = ownerId, UserName = ownerUserName, NormalizedUserName = normalizedOwnerUserName });
        context.Users.Add(new User { Id = newAssigneeId, UserName = newAssigneeUserName, NormalizedUserName = normalizedNewAssigneeUserName });
        context.Statuses.Add(new Data.Models.TaskStatus { TaskStatusId = 1, Status = Data.Models.TaskStatus.Statuses.ToDo });
        await context.SaveChangesAsync();

        var task = new Data.Models.Task
        {
            TaskId = taskId,
            Description = "Task to assign",
            TaskStatusId = 1,
            UserId = ownerId
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        currentUserService.Setup(x => x.GetUserId()).Returns(ownerId);

        var updateAssigneeDto = new UpdateTaskAssigneeDto { UserId = newAssigneeId };

        await service.UpdateTaskAssigneeAsync(taskId, updateAssigneeDto);

        var updatedTask = await context.Tasks
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TaskId == taskId);
        Assert.NotNull(updatedTask);
        Assert.Equal(newAssigneeId, updatedTask.UserId);
        Assert.Equal(newAssigneeUserName, updatedTask.User?.UserName);
    }

    [Fact]
    public async System.Threading.Tasks.Task ShouldThrowTaskNotFoundException_WhenTaskDoesNotExist()
    {
        var context = GetInMemoryDbContext();
        var currentUserService = new Mock<ICurrentUserService>();
        var service = new TaskService(context, currentUserService.Object);

        var userId = Guid.NewGuid().ToString();
        currentUserService.Setup(x => x.GetUserId()).Returns(userId);

        var updateAssigneeDto = new UpdateTaskAssigneeDto { UserId = userId };

        await Assert.ThrowsAsync<TaskNotFoundException>(() => service.UpdateTaskAssigneeAsync(999, updateAssigneeDto));
    }

    [Fact]
    public async System.Threading.Tasks.Task ShouldThrowUserNotFoundException_WhenAssigneeDoesNotExist()
    {
        var context = GetInMemoryDbContext();
        var currentUserService = new Mock<ICurrentUserService>();
        var service = new TaskService(context, currentUserService.Object);

        var ownerId = Guid.NewGuid().ToString();
        var ownerUserName = "Owner";
        var normalizedOwnerUserName = ownerUserName.ToUpper();

        var nonExistentAssigneeId = Guid.NewGuid().ToString();
        var taskId = 1;

        context.Users.Add(new User { Id = ownerId, UserName = ownerUserName, NormalizedUserName = normalizedOwnerUserName });
        context.Statuses.Add(new Data.Models.TaskStatus { TaskStatusId = 1, Status = Data.Models.TaskStatus.Statuses.ToDo });
        await context.SaveChangesAsync();

        var task = new Data.Models.Task
        {
            TaskId = taskId,
            Description = "Task to assign",
            TaskStatusId = 1,
            UserId = ownerId
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        currentUserService.Setup(x => x.GetUserId()).Returns(ownerId);

        var updateAssigneeDto = new UpdateTaskAssigneeDto { UserId = nonExistentAssigneeId };

        await Assert.ThrowsAsync<UserNotFoundException>(() => service.UpdateTaskAssigneeAsync(taskId, updateAssigneeDto));
    }

    [Fact]
    public async System.Threading.Tasks.Task ShouldThrowUserDoesNotOwnTaskException_WhenTaskExistsButUserDoesNotOwnIt()
    {
        var context = GetInMemoryDbContext();
        var currentUserService = new Mock<ICurrentUserService>();
        var service = new TaskService(context, currentUserService.Object);

        var ownerId = Guid.NewGuid().ToString();
        var ownerUserName = "Owner";
        var normalizedOwnerUserName = ownerUserName.ToUpper();

        var otherUserId = Guid.NewGuid().ToString();
        var otherUserName = "Other User";
        var normalizedOtherUserName = otherUserName.ToUpper();

        var newAssigneeId = Guid.NewGuid().ToString();
        var newAssigneeUserName = "New Assignee";
        var normalizedNewAssigneeUserName = newAssigneeUserName.ToUpper();
        var taskId = 1;

        context.Users.Add(new User { Id = ownerId, UserName = ownerUserName, NormalizedUserName = normalizedOwnerUserName });
        context.Users.Add(new User { Id = otherUserId, UserName = otherUserName, NormalizedUserName = normalizedOtherUserName });
        context.Users.Add(new User { Id = newAssigneeId, UserName = newAssigneeUserName, NormalizedUserName = normalizedNewAssigneeUserName });
        context.Statuses.Add(new Data.Models.TaskStatus { TaskStatusId = 1, Status = Data.Models.TaskStatus.Statuses.ToDo });
        await context.SaveChangesAsync();

        var task = new Data.Models.Task
        {
            TaskId = taskId,
            Description = "Not My Task",
            TaskStatusId = 1,
            UserId = ownerId
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        currentUserService.Setup(x => x.GetUserId()).Returns(otherUserId);

        var updateAssigneeDto = new UpdateTaskAssigneeDto { UserId = newAssigneeId };

        await Assert.ThrowsAsync<UserDoesNotOwnTaskException>(() => service.UpdateTaskAssigneeAsync(taskId, updateAssigneeDto));
    }
}