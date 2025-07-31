using System.Net;

namespace TaskManager.Domain.Exceptions;

public class TaskAlreadyHasThisStatusException : DomainException
{
    public TaskAlreadyHasThisStatusException() : base("Task already has this status.", (int)HttpStatusCode.Conflict)
    {
    }

    public TaskAlreadyHasThisStatusException(string message) : base(message, (int)HttpStatusCode.Conflict)
    {
    }
}