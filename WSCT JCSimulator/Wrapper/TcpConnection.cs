using System.Net;
using System.Net.Sockets;

namespace WSCT.JCSimulator.Wrapper;

public class TcpConnection(string ip = "127.0.0.1", int port = 9025) : IJcsConnection
{
    Socket? _socket;
    Stream? _stream;
    private bool disposedValue;

    #region >> IDisposable

    /// <inheritdoc />
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _stream?.Dispose();
                _socket?.Dispose();
            }

            disposedValue = true;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region >> IConnection

    /// <inheritdoc />
    /// <exception cref="JavaCardSimulatorException" />
    public Stream Connect()
    {
        var address = Dns.GetHostAddresses(ip, AddressFamily.InterNetwork).First();
        var endpoint = new IPEndPoint(address, port);

        try
        {
            _socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Connect to the client using specified endpoint
            _socket.Connect(endpoint);
        }
        catch (Exception e)
        {
            throw new JavaCardSimulatorException($"TCP Connection failed: {e.Message}");
        }

        _stream = new NetworkStream(_socket);

        return _stream;
    }

    /// <inheritdoc />
    public void Close()
    {
        _stream?.Close();
        _stream = null;

        _socket?.Disconnect(false);
        _socket?.Close();
        _socket = null;
    }

    #endregion
}
