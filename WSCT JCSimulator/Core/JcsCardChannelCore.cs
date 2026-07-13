using System.Text;
using WSCT.Core;
using WSCT.Core.APDU;
using WSCT.ISO7816;
using WSCT.JCSimulator.Wrapper;
using WSCT.Wrapper;

namespace WSCT.JCSimulator.Core;

/// <summary>
/// Represents a PC/SC channel object capable of communicating with the card running in a Java Card Simulator.
/// </summary>
/// <remarks>
/// Initializes a new instance.
/// </remarks>
public class JcsCardChannelCore(JcsClient jcSimulatorClient) : ICardChannel
{
    #region >> Fields

    private ICardContext? _context = null;
    private bool _connected = false;
    private byte[] _atr = [];

    #endregion
    #region >> Constructors

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
    public string ReaderName { get; private set; } = "Unknown";

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
            await jcSimulatorClient.SendAsync([0xF0, 0x00, 0x00, 0x00]);

            var block = await jcSimulatorClient.ReceiveDeviceResultAsync();

            return block[4..];
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

        Task.Run(async Task? () =>
        {
            await jcSimulatorClient.SendAsync([0xFE, 0x00, 0x00, 0x00]);
        })
            .GetAwaiter()
            .GetResult();

        _connected = false;

        return ErrorCode.Success;
    }

    /// <inheritdoc />
    public virtual ErrorCode GetAttrib(Attrib attrib, ref byte[] buffer)
    {
        buffer = attrib switch
        {
            Attrib.AtrString => _atr,
            Attrib.DeviceFriendlyName => Encoding.Default.GetBytes(ReaderName),
            _ => [],
        };
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
            await jcSimulatorClient.SendCommandAPDUAsync(new CommandAPDU(command.BinaryCommand));

            var bytes = await jcSimulatorClient.ReceiveInformationAsync();

            return bytes;
        })
            .GetAwaiter()
            .GetResult();

        response.Parse(bytes);

        return ErrorCode.Success;
    }

    #endregion
}
