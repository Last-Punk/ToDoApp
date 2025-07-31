using Moq;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Data.Models;
using TaskManager.Domain.Services;
using TaskManager.Domain.Exceptions;

namespace TaskManager.Tests;

public class GetTaskDetailsTests
{
    private Database GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<Database>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new Database(options);
    }

    [Fact]
    public async System.Threading.Tasks.Task ShouldGetTaskDetails_WhenTaskExistsAndUserOwnsIt()
    {
        var context = GetInMemoryDbContext();
        var currentUserService = new Mock<ICurrentUserService>();
        var service = new TaskService(context, currentUserService.Object);

        var userId = Guid.NewGuid().ToString();
        var userName = "Owner User";
        var normalizedUserName = userName.ToUpper();
        var taskId = 1;

        context.Users.Add(new User { Id = userId, UserName = userName, NormalizedUserName = normalizedUserName });
        context.Statuses.Add(new Data.Models.TaskStatus { TaskStatusId = 1, Status = Data.Models.TaskStatus.Statuses.ToDo });
        await context.SaveChangesAsync();

        var task = new Data.Models.Task
        {
            TaskId = taskId,
            Description = "Owned Task",
            DueDate = DateTime.UtcNow.AddDays(5),
            TaskStatusId = 1,
            UserId = userId
        };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        currentUserService.Setup(x => x.GetUserId()).Returns(userId);

        var result = await service.GetTaskDetailsAsync(taskId);

        Assert.NotNull(result);
        Assert.Equal(taskId, result.TaskId);
        Assert.Equal("Owned Task", result.Description);
        Assert.Equal(userName, result.AssignedToUser);
    }

    [Fact]
    public async System.Threading.Tasks.Task ShouldThrowTaskNotFoundException_WhenTaskDoesNotExist()
    {
        var context = GetInMemoryDbContext();
        var currentUserService = new Mock<ICurrentUserService>();
        var service = new TaskService(context, currentUserService.Object);

        var userId = Guid.NewGuid().ToString();
        currentUserService.Setup(x => x.GetUserId()).Returns(userId);

        await Assert.ThrowsAsync<TaskNotFoundException>(() => service.GetTaskDetailsAsync(999));
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

        await Assert.ThrowsAsync<UserDoesNotOwnTaskException>(() => service.GetTaskDetailsAsync(taskId));
    }
}
