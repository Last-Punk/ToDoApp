using System.Net;

namespace TaskManager.Domain.Exceptions;

public class UserDoesNotOwnTaskException : DomainException
{
    public UserDoesNotOwnTaskException() : base("User does not own this task.", (int)HttpStatusCode.Forbidden)
    {
    }

    public UserDoesNotOwnTaskException(string message) : base(message, (int)HttpStatusCode.Forbidden)
    {
    }
}