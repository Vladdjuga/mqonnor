namespace mqonnor.Application.Exceptions;

public class NoSuchCommandHandlerException:Exception
{
    public NoSuchCommandHandlerException(string message) : base(message){}
    public NoSuchCommandHandlerException(string message, Exception inner) : base(message, inner){}
}