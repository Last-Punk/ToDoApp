using Moq;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Data.Models;
using TaskManager.Domain.Services;
using Xunit;
using System;
using System.Threading.Tasks;

namespace TaskManager.Tests;

public class GetTasksTests
{
    private Database GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<Database>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new Database(options);
    }

    [Fact]
    public async System.Threading.Tasks.Task ShouldReturnOnlyTasksOwnedByCurrentUser()
    {
        var context = GetInMemoryDbContext();
        var currentUserService = new Mock<ICurrentUserService>();
        var service = new TaskService(context, currentUserService.Object);

        var userId1 = Guid.NewGuid().ToString();
        var userName1 = "User One";
        var normalizedUserName1 = userName1.ToUpper();

        var userId2 = Guid.NewGuid().ToString();
        var userName2 = "User Two";
        var normalizedUserName2 = userName2.ToUpper();

        context.Users.Add(new User { Id = userId1, UserName = userName1, NormalizedUserName = normalizedUserName1 });
        context.Users.Add(new User { Id = userId2, UserName = userName2, NormalizedUserName = normalizedUserName2 });
        context.Statuses.Add(new Data.Models.TaskStatus { TaskStatusId = 1, Status = Data.Models.TaskStatus.Statuses.ToDo });
        context.Statuses.Add(new Data.Models.TaskStatus { TaskStatusId = 2, Status = Data.Models.TaskStatus.Statuses.Done });
        await context.SaveChangesAsync();

        context.Tasks.AddRange(
            new Data.Models.Task { TaskId = 1, Description = "Task 1 (User 1)", UserId = userId1, TaskStatusId = 1 },
            new Data.Models.Task { TaskId = 2, Description = "Task 2 (User 2)", UserId = userId2, TaskStatusId = 1 },
            new Data.Models.Task { TaskId = 3, Description = "Task 3 (User 1)", UserId = userId1, TaskStatusId = 2 }
        );
        await context.SaveChangesAsync();

        currentUserService.Setup(x => x.GetUserId()).Returns(userId1);

        var tasks = await service.GetTasksAsync();

        Assert.NotNull(tasks);
        Assert.Equal(2, tasks.Count);
        Assert.Contains(tasks, t => t.Description == "Task 1 (User 1)");
        Assert.Contains(tasks, t => t.Description == "Task 3 (User 1)");
        Assert.DoesNotContain(tasks, t => t.Description == "Task 2 (User 2)");
    }

    [Fact]
    public async System.Threading.Tasks.Task ShouldReturnEmptyList_WhenNoTasksOwnedByCurrentUser()
    {
        var context = GetInMemoryDbContext();
        var currentUserService = new Mock<ICurrentUserService>();
        var service = new TaskService(context, currentUserService.Object);

        var userId1 = Guid.NewGuid().ToString();
        var userName1 = "User One";
        var normalizedUserName1 = userName1.ToUpper();

        var userId2 = Guid.NewGuid().ToString();

        context.Users.Add(new User { Id = userId1, UserName = userName1, NormalizedUserName = normalizedUserName1 });
        context.Statuses.Add(new Data.Models.TaskStatus { TaskStatusId = 1, Status = Data.Models.TaskStatus.Statuses.ToDo });
        await context.SaveChangesAsync();

        context.Tasks.Add(new Data.Models.Task { TaskId = 1, Description = "Task 1 (User 1)", UserId = userId1, TaskStatusId = 1 });
        await context.SaveChangesAsync();

        currentUserService.Setup(x => x.GetUserId()).Returns(userId2);

        var tasks = await service.GetTasksAsync();

        Assert.NotNull(tasks);
        Assert.Empty(tasks);
    }

    [Fact]
    public async System.Threading.Tasks.Task ShouldReturnAllTasks_IfUserIdIsNullInTaskModel_AndCurrentUserIdIsNull()
    {
        var context = GetInMemoryDbContext();
        var currentUserService = new Mock<ICurrentUserService>();
        var service = new TaskService(context, currentUserService.Object);

        context.Statuses.Add(new Data.Models.TaskStatus { TaskStatusId = 1, Status = Data.Models.TaskStatus.Statuses.ToDo });
        await context.SaveChangesAsync();

        context.Tasks.AddRange(
            new Data.Models.Task { TaskId = 1, Description = "Task 1 (No User)", UserId = null, TaskStatusId = 1 },
            new Data.Models.Task { TaskId = 2, Description = "Task 2 (No User)", UserId = null, TaskStatusId = 1 }
        );
        await context.SaveChangesAsync();

        currentUserService.Setup(x => x.GetUserId()).Returns((string)null);

        var tasks = await service.GetTasksAsync();

        Assert.NotNull(tasks);
        Assert.Equal(2, tasks.Count);
        Assert.Contains(tasks, t => t.Description == "Task 1 (No User)");
        Assert.Contains(tasks, t => t.Description == "Task 2 (No User)");
    }
}