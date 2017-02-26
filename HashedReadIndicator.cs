using System.Threading;

namespace SharpLeftRight
{
    class HashedReadIndicator : IReadIndicator
    {
        private int[] _occupancyCounts;
        private int _paddingPower;

        public HashedReadIndicator(int numEntries, int paddingPower)
        {
            _occupancyCounts = new int[numEntries << paddingPower];
            _paddingPower = paddingPower;
        }

        private int GetIndex()
        {
            var threadId = Thread.CurrentThread.ManagedThreadId; 
            var result = threadId.GetHashCode() << _paddingPower;
            return result;
        }
        
        public void Arrive()
        {
            int index = GetIndex();
            Interlocked.Increment(ref _occupancyCounts[index]);
        }

        public void Depart()
        {
            int index = GetIndex();
            Interlocked.Decrement(ref _occupancyCounts[index]);
        }

        public bool IsOccupied
        {
            get
            {
                // TODO: Memory fencing!
                for (int i = 0; i < _occupancyCounts.Length; ++i)
                {
                    if (_occupancyCounts[i << _paddingPower] > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}