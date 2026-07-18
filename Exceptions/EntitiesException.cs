namespace devault;

public class EntityException: Exception
{
    public EntityException() : base()
    {
        
    }

    public EntityException(string message): base(message)
    {
        
    }
    public EntityException(string message, Exception innerException):base(message, innerException)
    {
        
    }
}
