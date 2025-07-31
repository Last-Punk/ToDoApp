using Moq;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Data.Models;
using TaskManager.Domain.DTOs;
using TaskManager.Domain.Services;
using TaskManager.Domain.Exceptions;

namespace TaskManager.Tests;

public class UpdateTaskStatusTests
{
    private Database GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<Database>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new Database(options);
    }

    [Fact]
    public async System.Threading.Tasks.Task ShouldUpdateTaskStatus_WhenTaskExistsAndUserOwnsItAndNewStatusIsDifferent()
    {
        var context = GetInMemoryDbContext();
        var currentUserService = new Mock<ICurrentUserService>();
        var service = new TaskService(context, currentUserService.Object);

        var userId = Guid.NewGuid().ToString();
        var userName = "Test User";
        var normalizedUserName = userName.ToUpper();
        var taskId = 1;

        context.Users.Add(new User { Id = userId, UserName = userName, NormalizedUserName = normalizedUserName });
        context.Statuses.Add(new Data.Models.TaskStatus { TaskStatusId = 1, Status = Data.Models.TaskStatus.Statuses.ToDo });
        context.Statuses.Add(new Data.Models.TaskStatus { TaskStatusId = 2, Status = Data.Models.TaskStatus.Statuses.InProgress });
        await context.SaveChangesAsync();

        var task = new Data.Models.Task
        {
            TaskId = taskId,
            Description = "Task to update status",
            TaskStatusId = 1,
            UserId = userId
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        currentUserService.Setup(x => x.GetUserId()).Returns(userId);

        var updateStatusDto = new UpdateTaskStatusDto { NewStatus = Data.Models.TaskStatus.Statuses.InProgress };

        await service.UpdateTaskStatusAsync(taskId, updateStatusDto);

        var updatedTask = await context.Tasks.Include(t => t.TaskStatus).FirstOrDefaultAsync(t => t.TaskId == taskId);
        Assert.NotNull(updatedTask);
        Assert.Equal(Data.Models.TaskStatus.Statuses.InProgress, updatedTask.TaskStatus?.Status);
    }

    [Fact]
    public async System.Threading.Tasks.Task ShouldThrowTaskNotFoundException_WhenTaskDoesNotExist()
    {
        var context = GetInMemoryDbContext();
        var currentUserService = new Mock<ICurrentUserService>();
        var service = new TaskService(context, currentUserService.Object);

        var userId = Guid.NewGuid().ToString();
        currentUserService.Setup(x => x.GetUserId()).Returns(userId);

        var updateStatusDto = new UpdateTaskStatusDto { NewStatus = Data.Models.TaskStatus.Statuses.Done };

        await Assert.ThrowsAsync<TaskNotFoundException>(() => service.UpdateTaskStatusAsync(999, updateStatusDto));
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
        var taskId = 1;

        context.Users.Add(new User { Id = ownerId, UserName = ownerUserName, NormalizedUserName = normalizedOwnerUserName });
        context.Users.Add(new User { Id = otherUserId, UserName = otherUserName, NormalizedUserName = normalizedOtherUserName });
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

        var updateStatusDto = new UpdateTaskStatusDto { NewStatus = Data.Models.TaskStatus.Statuses.InProgress };

        await Assert.ThrowsAsync<UserDoesNotOwnTaskException>(() => service.UpdateTaskStatusAsync(taskId, updateStatusDto));
    }

    [Fact]
    public async System.Threading.Tasks.Task ShouldThrowTaskAlreadyHasThisStatusException_WhenTaskAlreadyHasTargetStatus()
    {
        var context = GetInMemoryDbContext();
        var currentUserService = new Mock<ICurrentUserService>();
        var service = new TaskService(context, currentUserService.Object);

        var userId = Guid.NewGuid().ToString();
        var userName = "Test User";
        var normalizedUserName = userName.ToUpper();
        var taskId = 1;

        context.Users.Add(new User { Id = userId, UserName = userName, NormalizedUserName = normalizedUserName });
        context.Statuses.Add(new Data.Models.TaskStatus { TaskStatusId = 1, Status = Data.Models.TaskStatus.Statuses.ToDo });
        await context.SaveChangesAsync();

        var task = new Data.Models.Task
        {
            TaskId = taskId,
            Description = "Task already ToDo",
            TaskStatusId = 1,
            UserId = userId
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        currentUserService.Setup(x => x.GetUserId()).Returns(userId);

        var updateStatusDto = new UpdateTaskStatusDto { NewStatus = Data.Models.TaskStatus.Statuses.ToDo };

        await Assert.ThrowsAsync<TaskAlreadyHasThisStatusException>(() => service.UpdateTaskStatusAsync(taskId, updateStatusDto));
    }
}