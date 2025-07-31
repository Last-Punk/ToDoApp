using System.Net;

namespace TaskManager.Domain.Exceptions;

public class TaskNotFoundException : DomainException
{
    public TaskNotFoundException() : base("Task not found.", (int)HttpStatusCode.NotFound)
    {
    }

    public TaskNotFoundException(string message) : base(message, (int)HttpStatusCode.NotFound)
    {
    }
}