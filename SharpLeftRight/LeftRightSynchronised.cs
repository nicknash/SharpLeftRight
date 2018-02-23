using System;

namespace SharpLeftRight
{
    public class LeftRightSynchronised<T>
    {
        private readonly T[] _instances;
        private readonly LeftRight _leftRight; 

        internal void SetSnoop(IInstanceSnoop snoop)
        {
            _leftRight.SetSnoop(snoop);
        }
        
        internal LeftRightSynchronised(T left, T right, LeftRight leftRight)
        {
            _instances = new[]{left, right};
            _leftRight = leftRight;
        }

        public U Read<U>(Func<T, U> read)
        {
            return _leftRight.Read(_instances, read);
        }

        public void Write(Action<T> write)
        {
            _leftRight.Write(_instances, write);
        }
    }
}