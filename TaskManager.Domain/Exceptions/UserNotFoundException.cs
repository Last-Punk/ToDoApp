using System.Net;

namespace TaskManager.Domain.Exceptions;

public class UserNotFoundException : DomainException
{
    public UserNotFoundException() : base("User not found.", (int)HttpStatusCode.NotFound)
    {
    }

    public UserNotFoundException(string message) : base(message, (int)HttpStatusCode.NotFound)
    {
    }
}