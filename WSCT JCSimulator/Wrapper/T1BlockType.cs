namespace WSCT.JCSimulator.Wrapper;

/// <summary>
/// T=1 Block type (ISO/IEC 7816 T=1 protocol)
/// </summary>
public enum T1BlockType
{
    /// <summary>Information T=1 Block</summary>
    IBlock,
    /// <summary>Received Ready T=1 Block</summary>
    RBlock,
    /// <summary>Supervision T=1 Block</summary>
    SBlock,
    /// <summary>Invalid T=1 Block</summary>
    Invalid
}
