using WSCT.JCSimulator.Core;
using WSCT.Stack;
using WSCT.Wrapper;

namespace WSCT.JCSimulator.Stack;

/// <summary>
/// This class implements a card context layer that adds Java Card Simulator support.
/// A dedicated fake reader is added to the context.
/// </summary>
public class JcsCardContextLayer : ICardContextLayer
{
    #region >> Fields

    private ICardContextStack? _stack;
    private JcsCardContextCore _context = new JcsCardContextCore();

    #endregion

    #region >> ICardContextLayer

    /// <inheritdoc/>
    public void SetStack(ICardContextStack stack)
    {
        this._stack = stack;
    }

    /// <inheritdoc />
    public string LayerId
    {
        get { return "WSCT JCSimulator"; }
    }

    #endregion

    #region >> ICardContext

    /// <inheritdoc />
    public IntPtr Context
    {
        get
        {
            JavaCardSimulatorLayerException.ThrowIfNull(_stack);

            return GetNextLayer(_stack)?.Context ?? IntPtr.Zero;
        }
    }

    /// <inheritdoc />
    public string[] Groups
    {
        get
        {
            JavaCardSimulatorLayerException.ThrowIfNull(_stack);

            var groups = GetNextLayer(_stack)?.Groups ?? ["WSCT Fake Reader Group"];

            groups = [.. groups, .. _context.Groups];

            return groups;
        }
    }

    /// <inheritdoc />
    public int GroupsCount
    {
        get
        {
            JavaCardSimulatorLayerException.ThrowIfNull(_stack);

            return GetNextLayer(_stack)?.GroupsCount ?? 1;
        }
    }

    /// <inheritdoc />
    public string[] Readers
    {
        get
        {
            JavaCardSimulatorLayerException.ThrowIfNull(_stack);

            var readers = GetNextLayer(_stack)?.Readers ?? [];

            readers = [.. readers, .. _context.Readers];

            return readers;
        }
    }

    /// <inheritdoc />
    public int ReadersCount
    {
        get
        {
            JavaCardSimulatorLayerException.ThrowIfNull(_stack);

            var readersCount = GetNextLayer(_stack)?.ReadersCount ?? 0;

            readersCount += _context.Readers.Count();

            return readersCount;
        }
    }

    /// <inheritdoc />
    public ErrorCode Cancel()
    {
        JavaCardSimulatorLayerException.ThrowIfNull(_stack);

        return GetNextLayer(_stack)?.Cancel() ?? ErrorCode.Success;
    }

    /// <inheritdoc />
    public ErrorCode Establish()
    {
        JavaCardSimulatorLayerException.ThrowIfNull(_stack);

        _context.Establish();

        return GetNextLayer(_stack)?.Establish() ?? ErrorCode.Success;
    }

    /// <inheritdoc />
    public ErrorCode GetStatusChange(uint timeout, AbstractReaderState[] readerStates)
    {
        // Filtering fakeReader
        var fakeReaderState = readerStates.FirstOrDefault(rs => _context.Readers.Contains(rs.ReaderName));

        // TODO: To be improved to wait for eventState occuring on fake reader ?
        if (fakeReaderState == null)
        {
            JavaCardSimulatorLayerException.ThrowIfNull(_stack);

            return GetNextLayer(_stack)?.GetStatusChange(timeout, readerStates) ?? ErrorCode.Success;
        }

        if (readerStates.Length <= 1)
        {
            return ErrorCode.Success;
        }

        // Sending getStatusChange of other readers to the next layer
        var filteredReaderStates = readerStates.Where(rs => !_context.Readers.Contains(rs.ReaderName)).ToArray();

        JavaCardSimulatorLayerException.ThrowIfNull(_stack);

        return GetNextLayer(_stack)?.GetStatusChange(timeout, filteredReaderStates) ?? ErrorCode.Success;
    }

    /// <inheritdoc />
    public ErrorCode IsValid()
    {
        JavaCardSimulatorLayerException.ThrowIfNull(_stack);

        return GetNextLayer(_stack)?.IsValid() ?? ErrorCode.Success;
    }

    /// <inheritdoc />
    public ErrorCode ListReaders(string group)
    {
        JavaCardSimulatorLayerException.ThrowIfNull(_stack);

        _context.ListReaders(group);

        var ret = GetNextLayer(_stack)?.ListReaders(group) ?? ErrorCode.Success;

        ret = ErrorCode.Success;

        return ret;
    }

    /// <inheritdoc />
    public ErrorCode ListReaderGroups()
    {
        JavaCardSimulatorLayerException.ThrowIfNull(_stack);

        _context.ListReaderGroups();

        return GetNextLayer(_stack)?.ListReaderGroups() ?? ErrorCode.Success;
    }

    /// <inheritdoc />
    public ErrorCode Release()
    {
        JavaCardSimulatorLayerException.ThrowIfNull(_stack);

        _context.Release();

        return GetNextLayer(_stack)?.Release() ?? ErrorCode.Success;
    }

    #endregion

    private ICardContextLayer? GetNextLayer(ICardContextStack stack)
    {
        try
        {
            return stack.RequestLayer(this, SearchMode.Next);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
