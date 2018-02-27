using System;
using RelaSharp.CLR;

namespace SharpLeftRight
{
    class LeftRight
    {
        private readonly Object _writersMutex = new Object();
        private IReadIndicator[] _readIndicators;
        private IWaitStrategy _waitStrategy;
        private CLRAtomicLong _versionIndex;
        private CLRAtomicLong _readIndex;
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
            var versionIndex = RUnordered.Read(ref _versionIndex);
            var readIndicator = _readIndicators[versionIndex];
            readIndicator.Arrive();     
            try
            {
                var readIndex = RInterlocked.Read(ref _readIndex);
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
                var readIndex = RUnordered.Read(ref _readIndex);
                var nextReadIndex = Flip(readIndex);
                try
                {
                    _snoop.BeginWrite(nextReadIndex);
                    write(instances[nextReadIndex]);
                    _snoop.EndWrite(nextReadIndex);
                }
                finally
                {
                    RInterlocked.Exchange(ref _readIndex, nextReadIndex);
                    var versionIndex = RUnordered.Read(ref _versionIndex);
                    var nextVersionIndex = Flip(versionIndex);
                    _waitStrategy.WaitWhileOccupied(_readIndicators[nextVersionIndex]);
                    RUnordered.Write(ref _versionIndex, nextVersionIndex);
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
