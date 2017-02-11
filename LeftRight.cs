using System;

namespace SharpLeftRight
{
    class LeftRight
    {
        private IReadIndicator[] _readIndicator;
        private IWaitStrategy _waitStrategy;
        private int _versionIndex;

        public WhereToRead ReadLocation { get; set; }

        public readonly Object WritersMutex;

        public LeftRight(IWaitStrategy waitStrategy)
        {
            _waitStrategy = waitStrategy;
        }


        // Traditional interface below here

        public int Arrive()
        {
            int localVersionIndex = _versionIndex; // TODO: Requires a memory fence
            _readIndicator[localVersionIndex].Arrive();
            return localVersionIndex;
        }

        public void ToggleVersionAndWait()
        {
            int localVersionIndex = _versionIndex; // TODO requires a memory fence
            int previousVersionIndex = localVersionIndex & 1;
            int nextVersionIndex = (localVersionIndex + 1) & 1;

            _waitStrategy.WaitWhileOccupied(_readIndicator[nextVersionIndex]);

            _versionIndex = nextVersionIndex; // TODO requires a memory fence

            _waitStrategy.WaitWhileOccupied(_readIndicator[previousVersionIndex]);

        }

        public void Depart(int versionIndex)
        {
            _readIndicator[versionIndex].Depart();
        }

    }
}
