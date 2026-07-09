namespace WSCT.JCSimulator.Wrapper;

public class NotConnectedException : Exception
{
    public NotConnectedException() : base("A successful Connect() must be called before")
    {
    }
}
