namespace TaskManager.Domain.Exceptions;

public class DomainException : Exception
{
    public int Status { get; }

    public DomainException(string message, int status) : base(message)
    {
        Status = status;
    }
}