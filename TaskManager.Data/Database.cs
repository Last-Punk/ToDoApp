using Microsoft.EntityFrameworkCore;
using TaskManager.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace TaskManager.Data;

public class Database : IdentityDbContext<User>
{
    public const string DatabaseProvider = "SQLite";

    public Database(DbContextOptions<Database> options)
        : base(options)
    {
    }
    public Database() : base()
    { 
    }

    public DbSet<Models.Task> Tasks { get; set; }
    public DbSet<Models.TaskStatus> Statuses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            if (DatabaseProvider == "SQLite")
            {
                string path = Path.Combine(
                    Environment.CurrentDirectory, "TaskManagerDB.db");
                optionsBuilder.UseSqlite($"Filename={path}");
            }
            else if (DatabaseProvider == "SQLServer")
            {
                string connection = "Data Source=.;" +
                    "InitialCatalog=TaskManagerDB;" +
                    "IntegratedSecurity=true;" +
                    "MultipleActiveResultSets=true;";
                optionsBuilder.UseSqlServer(connection);
            }
            else if (DatabaseProvider == "MySQL")
            {
                throw new NotImplementedException();
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Models.Task>()
            .HasOne(t => t.User)
            .WithMany(u => u.Tasks)
            .HasForeignKey(t => t.UserId);

        var passwordHasher = new PasswordHasher<User>();

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = "8e445865-a24d-4543-a6c6-9443d048cdb9",
                UserName = "johnsmith",
                NormalizedUserName = "JOHNSMITH",
                Email = "john@example.com",
                NormalizedEmail = "JOHN@EXAMPLE.COM",
                EmailConfirmed = true,
                PasswordHash = passwordHasher.HashPassword(null!, "Password123!"),
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            },
            new User
            {
                Id = "9e224968-33cd-4a37-b248-03290510526e",
                UserName = "maryjohnson",
                NormalizedUserName = "MARYJOHNSON",
                Email = "mary@example.com",
                NormalizedEmail = "MARY@EXAMPLE.COM",
                EmailConfirmed = true,
                PasswordHash = passwordHasher.HashPassword(null!, "Password123!"),
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            },
            new User
            {
                Id = "a9e69c3a-9c7b-4d7a-8f5c-2e8f7a6b5d4c",
                UserName = "alexbrown",
                NormalizedUserName = "ALEXBROWN",
                Email = "alex@example.com",
                NormalizedEmail = "ALEX@EXAMPLE.COM",
                EmailConfirmed = true,
                PasswordHash = passwordHasher.HashPassword(null!, "Password123!"),
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            }
        );

        modelBuilder.Entity<Models.TaskStatus>().HasData(
            new Models.TaskStatus { TaskStatusId = 1, Status = Models.TaskStatus.Statuses.ToDo },
            new Models.TaskStatus { TaskStatusId = 2, Status = Models.TaskStatus.Statuses.InProgress },
            new Models.TaskStatus { TaskStatusId = 3, Status = Models.TaskStatus.Statuses.InReview },
            new Models.TaskStatus { TaskStatusId = 4, Status = Models.TaskStatus.Statuses.Done },
            new Models.TaskStatus { TaskStatusId = 5, Status = Models.TaskStatus.Statuses.Paused },
            new Models.TaskStatus { TaskStatusId = 6, Status = Models.TaskStatus.Statuses.Failed }
        );

        modelBuilder.Entity<Models.Task>().HasData(
            new Models.Task
            {
                TaskId = 1,
                Description = "Develop user interface",
                TaskStatusId = 2,
                UserId = "8e445865-a24d-4543-a6c6-9443d048cdb9"
            },
            new Models.Task
            {
                TaskId = 2,
                Description = "Write authorization logic",
                TaskStatusId = 1,
                UserId = "9e224968-33cd-4a37-b248-03290510526e"
            },
            new Models.Task
            {
                TaskId = 3,
                Description = "Conduct functionality testing",
                TaskStatusId = 3,
                UserId = "8e445865-a24d-4543-a6c6-9443d048cdb9"
            },
            new Models.Task
            {
                TaskId = 4,
                Description = "Prepare documentation",
                TaskStatusId = 4,
                UserId = "a9e69c3a-9c7b-4d7a-8f5c-2e8f7a6b5d4c"
            },
            new Models.Task
            {
                TaskId = 5,
                Description = "Research new technologies",
                TaskStatusId = 5,
                UserId = "9e224968-33cd-4a37-b248-03290510526e"
            });
    }
}
