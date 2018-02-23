using System;
using RelaSharp.CLR;

namespace SharpLeftRight
{
    class LeftRight
    {
        private readonly Object _writersMutex = new Object();
        private IReadIndicator[] _readIndicators;
        private IWaitStrategy _waitStrategy;
        private MemoryOrdered _versionIndex = new MemoryOrdered(0);
        private MemoryOrdered _readIndex = new MemoryOrdered(0);
        private IInstanceSnoop _snoop = NullSnoop.Instance;
        
        internal void SetSnoop(IInstanceSnoop snoop)
        {
            _snoop = snoop;
        }

        public LeftRight(IWaitStrategy waitStrategy, IReadIndicator[] readIndicators)
        {
            _waitStrategy = waitStrategy;
            _readIndicators = readIndicators;
        }

        public U Read<T, U>(T[] instances, Func<T, U> read)
        {
            var versionIndex = _versionIndex.ReadUnordered;
            var readIndicator = _readIndicators[versionIndex];
            readIndicator.Arrive();     
            try
            {
                var readIndex = _readIndex.ReadSeqCst;
                _snoop.BeginRead(readIndex);
                var result = read(instances[readIndex]);
                _snoop.EndRead(readIndex);
                return result;
            }
            finally
            {
                readIndicator.Depart();
            }
        }

        public void Write<T>(T[] instances, Action<T> write)
        {
            RMonitor.Enter(_writersMutex);
            try
            {
                var readIndex = _readIndex.ReadUnordered;
                var nextReadIndex = Flip(readIndex);
                try
                {
                    _snoop.BeginWrite(nextReadIndex);
                    write(instances[nextReadIndex]);
                    _snoop.EndWrite(nextReadIndex);
                }
                finally
                {
                    _readIndex.WriteSeqCst(nextReadIndex);
                    var versionIndex = _versionIndex.ReadUnordered;
                    var nextVersionIndex = Flip(versionIndex);
                    _waitStrategy.WaitWhileOccupied(_readIndicators[nextVersionIndex]);
                    _versionIndex.WriteUnordered(nextVersionIndex);
                    _waitStrategy.WaitWhileOccupied(_readIndicators[versionIndex]);
                    _snoop.BeginWrite(readIndex);
                    write(instances[readIndex]);
                    _snoop.EndWrite(readIndex);
                }
            }
            finally
            {
                RMonitor.Exit(_writersMutex);
            }
        }
        private static long Flip(long i) => i ^ 1;
    }
}
