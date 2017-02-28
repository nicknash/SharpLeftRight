using System;

namespace SharpLeftRight
{
    class LeftRight
    {
        private readonly Object _writersMutex = new Object();
        
        private IReadIndicator[] _readIndicator;
        private IWaitStrategy _waitStrategy;
        private MemoryOrdered _versionIndex = new MemoryOrdered(0);

        private MemoryOrdered _readIndex = new MemoryOrdered(0);
        
        public LeftRight(IWaitStrategy waitStrategy)
        {
            _waitStrategy = waitStrategy;
        }

        public U Read<T, U>(T[] instances, Func<T, U> read)
        {
            var versionIndex = _versionIndex.ReadSeqCst;
            var readIndicator = _readIndicator[versionIndex];
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
                var readIndex = _readIndex.ReadSeqCst;
                var nextReadIndex = WrappedIncrement(readIndex);
                try
                {
                    write(instances[nextReadIndex]);
                }
                finally
                {
                    // TODO: Make these comments a more precise based on the Promela models.

                    // Move subsequent readers to 'next' instance 
                    _readIndex.WriteSeqCst(nextReadIndex);
                    // Wait for all readers marked in the 'next' read indicator,
                    // these readers could be reading the 'readIndex' instance 
                    // we want to write next 
                    var versionIndex = _versionIndex.ReadSeqCst;
                    var nextVersionIndex = WrappedIncrement(versionIndex);
                    _waitStrategy.WaitWhileOccupied(_readIndicator[nextVersionIndex]);
                    // Move subsequent readers to the 'next' read indicator 
                    _versionIndex.WriteSeqCst(nextVersionIndex);
                    // At this point all readers subsequent readers will read the 'next' instance
                    // and mark the 'nextVersionIndex' read indicator, so the only remaining potential
                    // readers are the ones on the 'versionIndex' read indicator, so wait for them to finish 
                    _waitStrategy.WaitWhileOccupied(_readIndicator[versionIndex]);
                    // At this point there may be readers, but they must be on nextReadIndex, we can 
                    // safely write.
                    write(instances[readIndex]);
                }
            }
        }
        private static long WrappedIncrement(long i)
        {
            return i ^ 1;
        }
    }
}
