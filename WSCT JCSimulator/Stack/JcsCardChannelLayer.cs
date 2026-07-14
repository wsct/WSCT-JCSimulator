using WSCT.Core;
using WSCT.Core.APDU;
using WSCT.JCSimulator.Core;
using WSCT.JCSimulator.Wrapper;
using WSCT.Stack;
using WSCT.Wrapper;

namespace WSCT.JCSimulator.Stack;

/// <summary>
/// This class implements a card channel layer that adds Java Card Simulator support.
/// The simulator is expected to be running and listening on 127.0.0.1:9025 (default <see cref="TcpConnection"/>).
/// </summary>
public class JcsCardChannelLayer : ICardChannelLayer
{
    #region >> Fields

    private ICardChannelStack? _stack;
    private JcsCardChannelCore? _jcsChannel;
    private bool _isSimulatorActive = false;

    #endregion

    #region >> ICardChannelLayer

    /// <inheritdoc />
    public void SetStack(ICardChannelStack stack)
    {
        this._stack = stack;
    }

    /// <inheritdoc />
    public string LayerId
    {
        get => "WSCT JCSimulator";
    }

    #endregion

    #region >> ICardChannel

    /// <inheritdoc />
    public Protocol Protocol
    {
        get
        {
            if (_isSimulatorActive)
            {
                JavaCardSimulatorLayerException.ThrowIfNull(_jcsChannel);

                // The simulator only supports T=1
                return Protocol.T1;
            }

            JavaCardSimulatorLayerException.ThrowIfNull(_stack);

            return _stack.RequestLayer(this, SearchMode.Next).Protocol;
        }
    }

    /// <inheritdoc />
    public string ReaderName
    {
        get
        {
            if (_isSimulatorActive)
            {
                JavaCardSimulatorLayerException.ThrowIfNull(_jcsChannel);

                return _jcsChannel.ReaderName;
            }

            JavaCardSimulatorLayerException.ThrowIfNull(_stack);

            return _stack.RequestLayer(this, SearchMode.Next).ReaderName;
        }
    }

    /// <inheritdoc />
    public void Attach(ICardContext context, string readerName)
    {
        _isSimulatorActive = (readerName == JcsCardContextCore.SimulatorReaderName);

        if (_isSimulatorActive)
        {
            var jcsClient = new JcsClient(new TcpConnection());
            Task.Run(jcsClient.ConnectToSimulator)
                .GetAwaiter()
                .GetResult();
            _jcsChannel = new JcsCardChannelCore(context, readerName, jcsClient);
            _isSimulatorActive = true;

            return;
        }

        _isSimulatorActive = false;
        _jcsChannel = null;

        JavaCardSimulatorLayerException.ThrowIfNull(_stack);

        _stack.RequestLayer(this, SearchMode.Next).Attach(context, readerName);
    }

    /// <inheritdoc />
    public ErrorCode Connect(ShareMode shareMode, Protocol preferedProtocol)
    {
        if (_isSimulatorActive)
        {
            JavaCardSimulatorLayerException.ThrowIfNull(_jcsChannel);

            return _jcsChannel.Connect(shareMode, preferedProtocol);
        }

        JavaCardSimulatorLayerException.ThrowIfNull(_stack);

        return _stack.RequestLayer(this, SearchMode.Next).Connect(shareMode, preferedProtocol);
    }

    /// <inheritdoc />
    public ErrorCode Disconnect(Disposition disposition)
    {
        if (_isSimulatorActive)
        {
            JavaCardSimulatorLayerException.ThrowIfNull(_jcsChannel);

            return _jcsChannel.Disconnect(disposition);
        }

        JavaCardSimulatorLayerException.ThrowIfNull(_stack);

        return _stack.RequestLayer(this, SearchMode.Next).Disconnect(disposition);
    }

    /// <inheritdoc />
    public ErrorCode GetAttrib(Attrib attrib, ref byte[] buffer)
    {
        if (_isSimulatorActive)
        {
            JavaCardSimulatorLayerException.ThrowIfNull(_jcsChannel);

            return _jcsChannel.GetAttrib(attrib, ref buffer);
        }

        JavaCardSimulatorLayerException.ThrowIfNull(_stack);

        return _stack.RequestLayer(this, SearchMode.Next).GetAttrib(attrib, ref buffer);
    }

    /// <inheritdoc />
    public State GetStatus()
    {
        if (_isSimulatorActive)
        {
            JavaCardSimulatorLayerException.ThrowIfNull(_jcsChannel);

            return _jcsChannel.GetStatus();
        }

        JavaCardSimulatorLayerException.ThrowIfNull(_stack);

        return _stack.RequestLayer(this, SearchMode.Next).GetStatus();
    }

    /// <inheritdoc />
    public ErrorCode Reconnect(ShareMode shareMode, Protocol preferedProtocol, Disposition initialization)
    {
        if (_isSimulatorActive)
        {
            JavaCardSimulatorLayerException.ThrowIfNull(_jcsChannel);

            return _jcsChannel.Reconnect(shareMode, preferedProtocol, initialization);
        }

        JavaCardSimulatorLayerException.ThrowIfNull(_stack);

        return _stack.RequestLayer(this, SearchMode.Next).Reconnect(shareMode, preferedProtocol, initialization);
    }

    /// <inheritdoc />
    public ErrorCode Transmit(ICardCommand command, ICardResponse response)
    {
        if (_isSimulatorActive)
        {
            JavaCardSimulatorLayerException.ThrowIfNull(_jcsChannel);

            return _jcsChannel.Transmit(command, response);
        }

        JavaCardSimulatorLayerException.ThrowIfNull(_stack);

        return _stack.RequestLayer(this, SearchMode.Next).Transmit(command, response);
    }

    #endregion
}
