using System;

namespace SharpLeftRight
{
    class LeftRightSynchronised<T>
    {
        private readonly T _left;
        private readonly T _right;
        private readonly LeftRight _leftRight;

        public LeftRightSynchronised(T left, T right, LeftRight leftRight)
        {
            _left = left;
            _right = right;
            _leftRight = leftRight;
        }

        public U Read<U>(Func<T, U> read)
        {
            int versionIndex = _leftRight.Arrive();
            var instance = _leftRight.ReadLocation == WhereToRead.ReadOnLeft ? _left : _right;
            var result = read(instance);
            _leftRight.Depart(versionIndex);
            return result;
        }

        public void Write(Action<T> write)
        {
            lock (_leftRight.WritersMutex)
            {
                if (_leftRight.ReadLocation == WhereToRead.ReadOnLeft)
                {
                    HandleFirstWrite(write, _right, _left, WhereToRead.ReadOnRight);
                }
                else
                {
                    HandleFirstWrite(write, _left, _right, WhereToRead.ReadOnLeft);
                }
            }
        }

        private void HandleFirstWrite(Action<T> write, T writeFirst, T writeSecond, WhereToRead nextToRead)
        {
            write(writeFirst);
            _leftRight.ReadLocation = nextToRead;
            _leftRight.ToggleVersionAndWait();
            write(writeSecond);
        }

    }
}