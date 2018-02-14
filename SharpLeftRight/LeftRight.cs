using System;

namespace SharpLeftRight
{
    class LeftRight
    {
        private readonly Object _writersMutex = new Object();
        private IReadIndicator[] _readIndicators;
        private IWaitStrategy _waitStrategy;
        private MemoryOrdered _versionIndex = new MemoryOrdered(0);
        private MemoryOrdered _readIndex = new MemoryOrdered(0);
        
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
                var result = read(instances[_readIndex.ReadSeqCst]);
                return result;
            }
            finally
            {
                readIndicator.Depart();
            }
        }

        public void Write<T>(T[] instances, Action<T> write)
        {
            lock (_writersMutex)
            {
                var readIndex = _readIndex.ReadUnordered;
                var nextReadIndex = Flip(readIndex);
                try
                {
                    write(instances[nextReadIndex]);
                }
                finally
                {
                    // Move subsequent readers to 'next' instance 
                    _readIndex.WriteSeqCst(nextReadIndex);
                    // Wait for all readers marked in the 'next' read indicator,
                    // these readers could be reading the 'readIndex' instance 
                    // we want to write next 
                    var versionIndex = _versionIndex.ReadUnordered;
                    var nextVersionIndex = Flip(versionIndex);
                    _waitStrategy.WaitWhileOccupied(_readIndicators[nextVersionIndex]);
                    // Move subsequent readers to the 'next' read indicator 
                    _versionIndex.WriteUnordered(nextVersionIndex);
                    // At this point all readers subsequent readers will read the 'next' instance
                    // and mark the 'nextVersionIndex' read indicator, so the only remaining potential
                    // readers are the ones on the 'versionIndex' read indicator, so wait for them to finish 
                    _waitStrategy.WaitWhileOccupied(_readIndicators[versionIndex]);
                    // At this point there may be readers, but they must be on nextReadIndex, we can 
                    // safely write.
                    write(instances[readIndex]);
                }
            }
        }
        private static long Flip(long i)
        {
            return i ^ 1;
        }
    }
}
