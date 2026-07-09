using WSCT.Core;
using WSCT.Wrapper;

namespace WSCT.JCSimulator.Core;

/// <summary>
/// <see cref="ICardContext"/> implementation proposing a fake reader to connect to the JavaCard Simulator.
/// </summary>
public class JcsCardContextCore : ICardContext
{
    #region >> Fields

    private bool _established = false;

    #endregion

    #region >> Constructor

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    public JcsCardContextCore()
    {
        _established = false;

        Context = IntPtr.Zero;
        Groups = [];
        Readers = [];
    }

    #endregion

    #region >> ICardContext Members

    /// <inheritdoc />
    public IntPtr Context { get; }

    /// <inheritdoc />
    public string[] Groups { get; private set; }

    /// <inheritdoc />
    public int GroupsCount
    {
        get { return Groups.Length; }
    }

    /// <inheritdoc />
    public string[] Readers { get; private set; }

    /// <inheritdoc />
    public int ReadersCount
    {
        get { return Readers.Length; }
    }

    /// <inheritdoc />
    public ErrorCode Cancel()
    {
        if (!_established)
        {
            return ErrorCode.ErrorInvalidHandle;
        }

        return ErrorCode.Success;
    }

    /// <inheritdoc />
    public virtual ErrorCode Establish()
    {
        if (_established)
        {
            return ErrorCode.Unexpected;
        }

        _established = true;

        return ErrorCode.Success;
    }

    /// <inheritdoc />
    public ErrorCode GetStatusChange(uint timeout, AbstractReaderState[] readerStates)
    {
        if (!_established)
        {
            return ErrorCode.ErrorInvalidHandle;
        }

        // TODO

        return ErrorCode.Success;
    }

    /// <inheritdoc />
    public ErrorCode IsValid()
    {
        return _established ? ErrorCode.Success : ErrorCode.InvalidHandle;
    }

    /// <inheritdoc />
    public virtual ErrorCode ListReaders(string group)
    {
        if (!_established)
        {
            return ErrorCode.ErrorInvalidHandle;
        }

        Readers = ["JavaCard Simulator Reader"];

        return ErrorCode.Success;
    }

    /// <inheritdoc />
    public virtual ErrorCode ListReaderGroups()
    {
        if (!_established)
        {
            return ErrorCode.ErrorInvalidHandle;
        }

        Groups = ["WSCT Fake Readers Group"];

        return ErrorCode.Success;
    }

    /// <inheritdoc />
    public virtual ErrorCode Release()
    {
        if (!_established)
        {
            return ErrorCode.ErrorInvalidHandle;
        }

        _established = false;

        return ErrorCode.Success;
    }

    #endregion
}
