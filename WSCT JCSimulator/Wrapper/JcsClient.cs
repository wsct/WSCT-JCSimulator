using WSCT.ISO7816;

namespace WSCT.JCSimulator.Wrapper;

/// <summary>
/// Client allowing to connect to a JavaCard Simulator using a given <see cref="IJcsConnection"/>.
/// </summary>
public class JcsClient(IJcsConnection connection) : IDisposable
{
    Stream? _stream;
    byte _ifsc = 0x20;
    byte _ifsd = 0x20;
    byte _sequenceNumber = 0;
    private bool disposedValue;

    #region >> IDisposable

    /// <inheritdoc />
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                connection?.Dispose();
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

    /// <summary>
    /// Bind to the given port and waits for a connection.
    /// </summary>
    public void ConnectToSimulator()
    {
        _stream = connection.Connect();

        // Sequence number is always 0 after a reset
        _sequenceNumber = 0;
    }

    /// <summary>
    /// Close the client (release all dedicated resources)
    /// </summary>
    public void DisconnectFromSimulator()
    {
        connection.Close();

        _stream?.Close();
        _stream = null;
    }

    /// <summary>
    /// Get the next incoming byte from the javacard simulator
    /// </summary>
    public async Task<byte> ReceiveByteAsync()
    {
        var buffer = await ReceiveBytesAsync(1);

        return buffer[0];
    }

    /// <summary>
    /// Get the next <paramref="count"/> incoming bytes from the javacard simulator
    /// </summary>
    public async Task<byte[]> ReceiveBytesAsync(int count)
    {
        JavaCardSimulatorException.ThrowIfNull(_stream);

        var buffer = new byte[count];
        var memory = buffer.AsMemory();

        int read = 0;

        while (read < count)
        {
            var chunkRead = await _stream.ReadAsync(memory[read..]);

            if (chunkRead == 0)
            {
                throw new JavaCardSimulatorException("JcsClient: The stream ended unexpectedly");
            }

            read += chunkRead;
        }

        return buffer;
    }

    /// <summary>
    /// Get the next incoming message from the javacard simulator
    /// </summary>
    public async Task<byte[]> ReceiveRawAsync()
    {
        JavaCardSimulatorException.ThrowIfNull(_stream);

        byte[] buffer = new byte[3 + _ifsd + 1];
        var read = await _stream.ReadAsync(buffer);

        return buffer[..read];
    }

    /// <summary>
    /// Get the next T=1 Block from the javacard simulator
    /// </summary>
    public async Task<T1Block> ReceiveT1BlockAsync()
    {
        JavaCardSimulatorException.ThrowIfNull(_stream);

        var nad = await ReceiveByteAsync();
        var pcb = await ReceiveByteAsync();
        var len = await ReceiveByteAsync();
        var info = await ReceiveBytesAsync(len);
        var lrc = await ReceiveByteAsync();

        return new T1Block([nad, pcb, len, .. info, lrc]);
    }

    /// <summary>
    /// Get the next T=1 Block from the javacard simulator
    /// </summary>
    public async Task<byte[]> ReceiveDeviceResultAsync()
    {
        JavaCardSimulatorException.ThrowIfNull(_stream);

        var command = await ReceiveByteAsync();
        var byte2 = await ReceiveByteAsync();
        var lenH = await ReceiveByteAsync();
        var lenL = await ReceiveByteAsync();
        var data = await ReceiveBytesAsync((lenH << 8) | lenL);

        return [command, byte2, lenH, lenL, .. data];
    }

    /// <summary>
    /// Get the next R-APDU from the javacard simulator
    /// </summary>
    public async Task<ResponseAPDU> ReceiveResponseApduAsync()
    {
        var rApduCompleted = false;
        var responseApdu = new List<byte>();

        do
        {
            var iBlockResponse = await ReceiveT1BlockAsync();

            if (iBlockResponse.BlockType == T1BlockType.IBlock)
            {
                responseApdu.AddRange(iBlockResponse.Info);
                if ((iBlockResponse.Pcb & 0x20) == 0x00)
                {
                    rApduCompleted = true;
                }
                else
                {
                    var nextExpectedSequenceNumber = (byte)(((iBlockResponse.Pcb & 0x40) >> 6) + 1);
                    var rBlockRequest = T1Block.CreateBlockR(nextExpectedSequenceNumber);
                    await SendAsync(rBlockRequest);
                }
            }
        } while (!rApduCompleted);

        return new ResponseAPDU([.. responseApdu]);
    }

    /// <summary>
    /// Get the next R-APDU from the javacard simulator
    /// </summary>
    public async Task<byte[]> ReceiveInformationAsync()
    {
        var chainingCompleted = false;
        var information = new List<byte>();

        do
        {
            var iBlockResponse = await ReceiveT1BlockAsync();

            if (iBlockResponse.BlockType == T1BlockType.IBlock)
            {
                information.AddRange(iBlockResponse.Info);

                if ((iBlockResponse.Pcb & 0x20) == 0x00)
                {
                    chainingCompleted = true;
                }
                else
                {
                    var nextExpectedSequenceNumber = (byte)(((iBlockResponse.Pcb & 0x40) >> 6) + 1);
                    var rBlockRequest = T1Block.CreateBlockR(nextExpectedSequenceNumber);
                    await SendAsync(rBlockRequest);
                }
            }
            else
            {
                // Something went wrong: abort
                chainingCompleted = true;
            }
        } while (!chainingCompleted);

        return [.. information];
    }

    /// <summary>
    /// Send a message to the javacard simulator
    /// </summary>
    public async Task SendAsync(IReadOnlyCollection<byte> message)
    {
        JavaCardSimulatorException.ThrowIfNull(_stream);

        await _stream.WriteAsync(message.ToArray());

        await _stream.FlushAsync();
    }

    /// <summary>
    /// Send a T=1 Block to the javacard simulator
    /// </summary>
    public async Task SendAsync(T1Block block)
    {
        JavaCardSimulatorException.ThrowIfNull(_stream);

        await _stream.WriteAsync(block.Block);

        await _stream.FlushAsync();
    }

    /// <summary>
    /// Send a C-APDU to the javacard simulator
    /// </summary>
    public async Task SendCommandAPDUAsync(CommandAPDU apdu)
    {
        var chunks = apdu.BinaryCommand.Chunk(_ifsc).ToArray();

        foreach (var chunk in chunks[..^1])
        {
            T1Block chainedBlock = T1Block.CreateBlockI(chunk, sequence: _sequenceNumber, chaining: true);

            await SendAsync(chainedBlock);

            _sequenceNumber = (byte)((_sequenceNumber + 1) % 2);

            var rBlock = await ReceiveT1BlockAsync();

            var expectedSequenceNumber = (byte)((_sequenceNumber + 1) % 2);
            if (rBlock.BlockType != T1BlockType.RBlock || rBlock.GetRBlockSequenceNumber() != expectedSequenceNumber)
            {
                throw new JavaCardSimulatorException($"JcsClient: Expected R({expectedSequenceNumber}) from the simulator");
            }

            // TODO Should check LRC
        }

        var lastBlock = T1Block.CreateBlockI(chunks[^1], sequence: _sequenceNumber, chaining: false);

        await SendAsync(lastBlock);

        _sequenceNumber = (byte)((_sequenceNumber + 1) % 2);
    }

    /// <summary>
    /// Send the T=1 IFSD value and receive the T=1 IFSC
    /// </summary>
    public async Task SendIfsAsync(byte ifsd)
    {
        var ifsRequestBlock = T1Block.CreateBlockS([ifsd], true, 0x01);
        await SendAsync(ifsRequestBlock);

        var ifsResponseblock = await ReceiveT1BlockAsync();

        _ifsd = ifsd;

        if (ifsResponseblock.Len > 0)
        {
            _ifsc = ifsRequestBlock.Info[0];
        }
    }
}
