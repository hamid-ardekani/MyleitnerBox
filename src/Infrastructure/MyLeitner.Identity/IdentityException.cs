namespace MyLeitner.Identity;

public class IdentityException:Exception
{
    public IdentityException(string message) : base(message)
    {
        
    }
    
    public IdentityException(string message, object value) : base(string.Format(message, value))
    {
        
    }
}