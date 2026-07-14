namespace WSCT.JCSimulator.Wrapper;

/// <summary>
/// T=1 Block structure (ISO/IEC 7816 T=1 protocol)
/// </summary>
public record T1Block(byte[] Block)
{
    /// <summary>
    /// NAD: Node Address Byte (ISO/IEC 7816 T=1 protocol)
    /// </summary>
    public byte Nad => Block[0];

    /// <summary>
    /// PCB: Protocol Control Block (ISO/IEC 7816 T=1 protocol)
    /// </summary>
    public byte Pcb => Block[1];

    /// <summary>
    /// LEN: Length of the INFORMATION field (ISO/IEC 7816 T=1 protocol)
    /// </summary>
    public byte Len => Block[2];

    /// <summary>
    /// INFORMATION field
    /// </summary>
    public Span<byte> Info => Block.AsSpan()[3..^1];

    /// <summary>
    /// LRC: Longitudinal Redundancy Check (ISO/IEC 7816 T=1 protocol).<br/>
    /// CRC is not supported.
    /// </summary>
    public byte Lrc => Block[^1];

    /// <summary>
    /// T=1 Block Type (Information / Receive Ready / Supervision)
    /// </summary>
    public T1BlockType BlockType => GetBlockType(Pcb);

    /// <summary>
    /// Get the T=1 block type (I / R / S) based of <paramref="pcb" />
    /// </summary>
    private static T1BlockType GetBlockType(byte pcb)
    {
        if ((pcb & 0x80) == 0x00)
        {
            return T1BlockType.IBlock;
        }
        return (pcb & 0xC0) switch
        {
            0x80 => T1BlockType.RBlock,
            0xC0 => T1BlockType.SBlock,
            _ => T1BlockType.Invalid
        };

    }

    /// <summary>
    /// Create a new T=1 I-Block (Information)
    /// </summary>
    public static T1Block CreateBlockI(Span<byte> info, byte sequence, bool chaining = false)
    {
        var pcb = ((sequence % 2) << 6) | (chaining ? 0x20 : 0x00);

        var lrc = 0x00 ^ pcb ^ info.Length;
        foreach (var b in info)
        {
            lrc ^= b;
        }

        return new T1Block([0x00, (byte)pcb, (byte)info.Length, .. info, (byte)lrc]);
    }

    /// <summary>
    /// Create a new T=1 R-Block (Receive Ready)
    /// </summary>
    public static T1Block CreateBlockR(byte sequence, byte error = 0x00)
    {
        var pcb = 0x80 | ((sequence % 2) << 4) | error;

        byte[] info = [];

        var lrc = (byte)(0x00 ^ pcb ^ 0x00);

        return new T1Block([0x00, (byte)pcb, 0x00, .. info, lrc]);
    }

    /// <summary>
    /// Create a new T=1 S-Block (Supervision)
    /// </summary>
    public static T1Block CreateBlockS(Span<byte> info, bool isRequest, byte type = 0x01)
    {
        var pcb = 0xC0 | (isRequest ? 0x00 : 0x20) | (type & 0x03);

        var lrc = 0x00 ^ pcb ^ info.Length;
        foreach (var b in info)
        {
            lrc ^= b;
        }

        return new T1Block([0x00, (byte)pcb, (byte)info.Length, .. info, (byte)lrc]);
    }

    public byte GetRBlockSequenceNumber()
    {
        return (byte)((Pcb & 0x10) >> 4);
    }
}
