namespace WSCT.JCSimulator.Wrapper;

public interface IConnection : IDisposable
{
    /// <summary>
    /// Open the connection to the Java Card Simulator.
    /// </summary>
    Stream Open();

    /// <summary>
    /// Close the connection to the Java Card Simulator.
    /// </summary>
    void Close();
}
