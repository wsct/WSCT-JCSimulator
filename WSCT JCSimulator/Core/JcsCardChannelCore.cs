using System.Text;
using WSCT.Core;
using WSCT.Core.APDU;
using WSCT.ISO7816;
using WSCT.JCSimulator.Wrapper;
using WSCT.Wrapper;

namespace WSCT.JCSimulator.Core;

public class JcsCardChannelCore : ICardChannel
{
    #region >> Fields

    private ICardContext? _context = null;
    private bool _connected = false;
    private byte[] _atr = [];

    private JcsClient _jcSimulatorClient;

    #endregion

    #region >> Constructors

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    public JcsCardChannelCore(JcsClient jcSimulatorClient)
    {
        _connected = false;
        _jcSimulatorClient = jcSimulatorClient;
        ReaderName = "Unknown";
    }

    /// <summary>
    /// Constructor (<seealso cref="Attach"/>).
    /// </summary>
    /// <param name="context">Resource manager context to attach.</param>
    /// <param name="readerName">Name of the reader to use.</param>
    /// <param name="fakeCard"></param>
    public JcsCardChannelCore(ICardContext context, string readerName, JcsClient jcSimulatorClient)
        : this(jcSimulatorClient)
    {
        Attach(context, readerName);
    }

    #endregion

    #region >> ICardChannel Members

    /// <inheritdoc />
    public Protocol Protocol { get; private set; }

    /// <inheritdoc />
    public string ReaderName { get; private set; }

    /// <inheritdoc />
    public virtual void Attach(ICardContext context, string readerName)
    {
        _context = context;
        ReaderName = readerName;
        Protocol = Protocol.T0;
    }

    /// <inheritdoc />
    public virtual ErrorCode Connect(ShareMode shareMode, Protocol preferredProtocol)
    {
        Protocol = preferredProtocol;

        _atr = Task.Run(async Task<byte[]>? () =>
        {
            await _jcSimulatorClient.SendAsync([0xF0, 0x00, 0x00, 0x00]);

            var block = await _jcSimulatorClient.ReceiveDeviceResultAsync();

            return block[4..].ToArray();
        })
            .GetAwaiter()
            .GetResult();

        _connected = true;

        return ErrorCode.Success;
    }

    /// <inheritdoc />
    public virtual ErrorCode Disconnect(Disposition disposition)
    {
        if (!_connected)
        {
            return ErrorCode.ErrorInvalidHandle;
        }

        Task.Run(async Task<byte[]>? () =>
        {
            await _jcSimulatorClient.SendAsync([0xFE, 0x00, 0x00, 0x00]);

            var block = await _jcSimulatorClient.ReceiveDeviceResultAsync();

            return block[4..];
        })
            .GetAwaiter()
            .GetResult();

        _connected = false;

        return ErrorCode.Success;
    }

    /// <inheritdoc />
    public virtual ErrorCode GetAttrib(Attrib attrib, ref byte[] buffer)
    {
        switch (attrib)
        {
            case Attrib.AtrString:
                buffer = _atr;
                break;
            case Attrib.DeviceFriendlyName:
                buffer = Encoding.Default.GetBytes(ReaderName);
                break;
            default:
                buffer = Array.Empty<byte>();
                break;
        }

        return ErrorCode.Success;
    }

    /// <inheritdoc />
    public virtual State GetStatus()
    {
        // TODO

        return State.Present;
    }

    /// <inheritdoc />
    public virtual ErrorCode Reconnect(ShareMode shareMode, Protocol preferedProtocol, Disposition disposition)
    {
        Disconnect(disposition);

        Connect(shareMode, preferedProtocol);

        Protocol = preferedProtocol;

        return ErrorCode.Success;
    }

    /// <inheritdoc />
    public virtual ErrorCode Transmit(ICardCommand command, ICardResponse response)
    {
        var bytes = Task.Run(async Task<byte[]>? () =>
        {
            await _jcSimulatorClient.SendCommandAPDUAsync(new CommandAPDU(command.BinaryCommand));

            var bytes = await _jcSimulatorClient.ReceiveInformationAsync();

            return bytes;
        })
            .GetAwaiter()
            .GetResult();

        response.Parse(bytes);

        return ErrorCode.Success;
    }

    #endregion
}
