using System.Threading;
using RelaSharp.CLR;

namespace SharpLeftRight
{
    class HashedReadIndicator : IReadIndicator
    {
        private CLRAtomicInt[] _occupancyCounts;
        private readonly int _paddingPower;
        private readonly int _numEntries;

        public HashedReadIndicator(int numEntries, int paddingPower)
        {
            _occupancyCounts = new CLRAtomicInt[numEntries << paddingPower];
            _numEntries = numEntries;
            _paddingPower = paddingPower;
        }

        private int GetIndex()
        {
            var threadId = Thread.CurrentThread.ManagedThreadId; 
            var result = (threadId.GetHashCode() << _paddingPower) % _numEntries;
            return result;
        }
        
        public void Arrive()
        {
            int index = GetIndex();
            RInterlocked.Increment(ref _occupancyCounts[index]);
        }

        public void Depart()
        {
            int index = GetIndex();
            RInterlocked.Decrement(ref _occupancyCounts[index]);
        }

        public bool IsOccupied
        {
            get
            {
                // TODO: Memory fencing!
                for (int i = 0; i < _numEntries; ++i)
                {
                    if (RVolatile.Read(ref _occupancyCounts[i << _paddingPower]) > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}