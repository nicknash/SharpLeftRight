using System;
using System.Threading;

namespace SharpLeftRight
{
    class MemoryOrdered
    {
        private long _value;
        public MemoryOrdered(long initialValue)
        {
            _value = initialValue;
        }

        public long ReadSeqCst
        {
            get
            {
                Interlocked.Read(ref _value);
                return _value;
            }
        }
        public void WriteSeqCst(long newValue)
        {
            Interlocked.Exchange(ref _value, newValue);
        }
    }
    struct ReadContext<T>
    {
        private readonly IReadIndicator _readIndicator;

        public readonly T ReadInstance;

        public ReadContext(IReadIndicator readIndicator, T readInstance)
        {
            _readIndicator = readIndicator;
            ReadInstance = readInstance;
        }
        public void ReadCompleted()
        {
            _readIndicator.Depart();
        }
    }

    struct WriteContext<T>
    {
        public readonly long _nextReadIndex;

        public readonly LeftRight _leftRight;
        public readonly T FirstWriteTarget;
        public readonly T SecondWriteTarget;

        public WriteContext(long nextReadIndex, T firstWriteTarget, T secondWriteTarget, LeftRight leftRight)
        {
            _nextReadIndex = nextReadIndex;
            _leftRight = leftRight;
            FirstWriteTarget = firstWriteTarget;
            SecondWriteTarget = secondWriteTarget;
        }

        public void WaitBeforeSecondWrite()
        {
            _leftRight.WaitBeforeSecondWrite(_nextReadIndex);
        }
    }
    class LeftRight
    {
        private IReadIndicator[] _readIndicator;
        private IWaitStrategy _waitStrategy;
        private MemoryOrdered _versionIndex = new MemoryOrdered(0);

        private MemoryOrdered _readIndex = new MemoryOrdered(0);
        
        public readonly Object WritersMutex;

        public LeftRight(IWaitStrategy waitStrategy)
        {
            _waitStrategy = waitStrategy;
        }

        public ReadContext<T> BeginRead<T>(T[] instances)
        {
            var versionIndex = _versionIndex.ReadSeqCst;
            var readIndicator = _readIndicator[versionIndex];
            readIndicator.Arrive();
            return new ReadContext<T>(readIndicator, instances[_readIndex.ReadSeqCst]);
        }
        public WriteContext<T> BeginWrite<T>(T[] instances)
        {
            var readIndex = _readIndex.ReadSeqCst;
            var nextReadIndex = WrappedIncrement(readIndex);
            return new WriteContext<T>(nextReadIndex, instances[nextReadIndex], instances[readIndex], this);
        }

        internal void WaitBeforeSecondWrite(long nextReadIndex)
        {
            _readIndex.WriteSeqCst(nextReadIndex);
            var versionIndex = _versionIndex.ReadSeqCst;
            var nextVersionIndex = WrappedIncrement(versionIndex);

            _waitStrategy.WaitWhileOccupied(_readIndicator[nextVersionIndex]);
            _versionIndex.WriteSeqCst(nextVersionIndex);
            _waitStrategy.WaitWhileOccupied(_readIndicator[versionIndex]);

        }
        private static long WrappedIncrement(long i)
        {
            return i ^ 1;
        }
    }
}
