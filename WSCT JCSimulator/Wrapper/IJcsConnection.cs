namespace WSCT.JCSimulator.Wrapper;

public interface IJcsConnection : IDisposable
{
    /// <summary>
    /// Open the connection to the Java Card Simulator.
    /// </summary>
    Stream Connect();

    /// <summary>
    /// Close the connection to the Java Card Simulator.
    /// </summary>
    void Close();
}
