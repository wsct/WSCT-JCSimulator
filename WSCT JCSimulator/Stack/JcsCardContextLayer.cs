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

                return _stack.RequestLayer(this, SearchMode.Next).Context;
            }
        }

        /// <inheritdoc />
        public string[] Groups
        {
            get
            {
                JavaCardSimulatorLayerException.ThrowIfNull(_stack);

                return _stack.RequestLayer(this, SearchMode.Next).Groups;
            }
        }

        /// <inheritdoc />
        public int GroupsCount
        {
            get
            {
                JavaCardSimulatorLayerException.ThrowIfNull(_stack);

                return _stack.RequestLayer(this, SearchMode.Next).GroupsCount;
            }
        }

        /// <inheritdoc />
        public string[] Readers
        {
            get
            {
                JavaCardSimulatorLayerException.ThrowIfNull(_stack);

                var readers = _stack.RequestLayer(this, SearchMode.Next).Readers;

                readers = [.. _context.Readers, .. readers];

                return readers;
            }
        }

        /// <inheritdoc />
        public int ReadersCount
        {
            get
            {
                JavaCardSimulatorLayerException.ThrowIfNull(_stack);

                var readersCount = _stack.RequestLayer(this, SearchMode.Next).ReadersCount;

                readersCount += _context.Readers.Count();

                return readersCount;
            }
        }

        /// <inheritdoc />
        public ErrorCode Cancel()
        {
            JavaCardSimulatorLayerException.ThrowIfNull(_stack);

            return _stack.RequestLayer(this, SearchMode.Next).Cancel();
        }

        /// <inheritdoc />
        public ErrorCode Establish()
        {
            JavaCardSimulatorLayerException.ThrowIfNull(_stack);

            _context.Establish();

            return _stack.RequestLayer(this, SearchMode.Next).Establish();
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

                return _stack.RequestLayer(this, SearchMode.Next).GetStatusChange(timeout, readerStates);
            }

            if (readerStates.Length <= 1)
            {
                return ErrorCode.Success;
            }

            // Sending getStatusChange of other readers to the next layer
            var filteredReaderStates = readerStates.Where(rs => !_context.Readers.Contains(rs.ReaderName)).ToArray();

            JavaCardSimulatorLayerException.ThrowIfNull(_stack);

            return _stack.RequestLayer(this, SearchMode.Next).GetStatusChange(timeout, filteredReaderStates);
        }

        /// <inheritdoc />
        public ErrorCode IsValid()
        {
            JavaCardSimulatorLayerException.ThrowIfNull(_stack);

            return _stack.RequestLayer(this, SearchMode.Next).IsValid();
        }

        /// <inheritdoc />
        public ErrorCode ListReaders(string group)
        {
            JavaCardSimulatorLayerException.ThrowIfNull(_stack);

            _context.ListReaders(group);

            var ret = _stack.RequestLayer(this, SearchMode.Next).ListReaders(group);

            ret = ErrorCode.Success;

            return ret;
        }

        /// <inheritdoc />
        public ErrorCode ListReaderGroups()
        {
            JavaCardSimulatorLayerException.ThrowIfNull(_stack);

            _context.ListReaderGroups();

            return _stack.RequestLayer(this, SearchMode.Next).ListReaderGroups();
        }

        /// <inheritdoc />
        public ErrorCode Release()
        {
            JavaCardSimulatorLayerException.ThrowIfNull(_stack);

            _context.Release();

            return _stack.RequestLayer(this, SearchMode.Next).Release();
        }

        #endregion
    }
