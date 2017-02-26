using System;

namespace SharpLeftRight
{
    class LeftRightSynchronised<T>
    {
        private readonly T[] _instances;
        private readonly LeftRight _leftRight; 
        
        public LeftRightSynchronised(T left, T right, LeftRight leftRight)
        {
            _instances = new[]{left, right};
            _leftRight = leftRight;
        }

        public U Read<U>(Func<T, U> read)
        {
            var readContext = _leftRight.BeginRead(_instances);
            try
            {
                var result = read(readContext.ReadInstance);
                return result;
            }
            finally
            {
                readContext.ReadCompleted();
            }
        }

        public void Write(Action<T> write)
        {
            lock (_leftRight.WritersMutex)
            {
                var writeContext = _leftRight.BeginWrite(_instances);
                try
                {
                    write(writeContext.FirstWriteTarget);
                }
                finally
                {
                    writeContext.WaitBeforeSecondWrite();
                    write(writeContext.SecondWriteTarget);
                }
            }
        }
    }
}