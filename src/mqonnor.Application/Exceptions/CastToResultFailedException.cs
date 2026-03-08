namespace mqonnor.Application.Exceptions;

public class CastToResultFailedException:InvalidOperationException
{
    public CastToResultFailedException(string message) : base(message) { }
    public CastToResultFailedException(string message, Exception innerException) : base(message, innerException) { }
}