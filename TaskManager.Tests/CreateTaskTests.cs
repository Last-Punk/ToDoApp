using Moq;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Data.Models;
using TaskManager.Domain.DTOs;
using TaskManager.Domain.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace TaskManager.Tests;

public class CreateTaskTests
{
    private Database GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<Database>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new Database(options);
    }

    [Fact]
    public async System.Threading.Tasks.Task ShouldCreateTask_WhenValidDataProvided()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var currentUserService = new Mock<ICurrentUserService>();
        var service = new TaskService(context, currentUserService.Object);

        var userId = Guid.NewGuid().ToString();
        var userName = "Test User Name";
        var normalizedUserName = userName.ToUpper();

        context.Users.Add(new User
        {
            Id = userId,
            UserName = userName,
            NormalizedUserName = normalizedUserName
        });
        await context.SaveChangesAsync();

        currentUserService.Setup(x => x.GetUserId()).Returns(userId);

        var createTaskDto = new CreateTaskDto
        {
            Description = "New Test Task",
            DueDate = DateTime.UtcNow.AddDays(7),
            UserId = userId
        };

        // Act
        await service.CreateTaskAsync(createTaskDto);

        // Assert
        var createdTask = await context.Tasks
            .Include(t => t.TaskStatus)
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Description == "New Test Task");

        Assert.NotNull(createdTask);
        Assert.Equal("New Test Task", createdTask.Description);
        Assert.Equal(createTaskDto.DueDate.Value.Date, createdTask.DueDate.Value.Date);
        Assert.Equal(Data.Models.TaskStatus.Statuses.ToDo, createdTask.TaskStatus?.Status);
        Assert.Equal(userId, createdTask.UserId);
        Assert.Equal(userName, createdTask.User?.UserName);
    }

    [Fact]
    public async System.Threading.Tasks.Task ShouldCreateTask_WithNullDueDate()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var currentUserService = new Mock<ICurrentUserService>();
        var service = new TaskService(context, currentUserService.Object);

        var userId = Guid.NewGuid().ToString();
        var userName = "User Without DueDate";
        var normalizedUserName = userName.ToUpper();

        context.Users.Add(new User { Id = userId, UserName = userName, NormalizedUserName = normalizedUserName });
        await context.SaveChangesAsync();
        currentUserService.Setup(x => x.GetUserId()).Returns(userId);

        var createTaskDto = new CreateTaskDto
        {
            Description = "Task without due date",
            DueDate = null,
            UserId = userId
        };

        // Act
        await service.CreateTaskAsync(createTaskDto);

        // Assert
        var createdTask = await context.Tasks.FirstOrDefaultAsync(t => t.Description == "Task without due date");
        Assert.NotNull(createdTask);
        Assert.Null(createdTask.DueDate);
    }

    [Fact]
    public async System.Threading.Tasks.Task ShouldCreateTask_WithNoAssignedUser_IfUserIdIsNullFromCurrentUserService()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var currentUserService = new Mock<ICurrentUserService>();
        var service = new TaskService(context, currentUserService.Object);

        var createTaskDto = new CreateTaskDto
        {
            Description = "Task for no user",
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        await service.CreateTaskAsync(createTaskDto);

        // Assert
        var createdTask = await context.Tasks.FirstOrDefaultAsync(t => t.Description == "Task for no user");
        Assert.NotNull(createdTask);
        Assert.Null(createdTask.UserId);
    }
}
