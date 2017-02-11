using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SharpLeftRight
{
    interface IReadIndicator
    {
        void Arrive();
        void Depart();
        bool IsOccupied();
    }

    interface IWaitStrategy
    {
        void WaitWhileOccupied(IReadIndicator readIndicator);
    }

    struct ReadToken<T> : IDisposable
    {
        public readonly T ReadInstance;

        private readonly LeftRight _leftRight;
        private readonly int _versionIndex;

        public ReadToken(LeftRight leftRight, int versionIndex, T readInstance)
        {        
            _leftRight = leftRight;
            ReadInstance = readInstance;
            _versionIndex = versionIndex;
        }

        public void EndRead()
        {
            _leftRight.Depart(_versionIndex);
        }

        public void Dispose()
        {
            EndRead();
        }
    }

    struct WriteToken<T> : IDisposable 
    {
        private readonly T _writeSecond;
        public readonly T WriteFirst;

        private readonly WhereToRead _nextRead;

        private readonly LeftRight _leftRight;

        public WriteToken(T writeFirst, T writeSecond, WhereToRead nextRead, LeftRight leftRight)
        {
            WriteFirst = writeFirst;
            _writeSecond = writeSecond;
            _nextRead = nextRead;
            _leftRight = leftRight;
        }

        public T EndFirstWrite()
        {
            _leftRight.ReadLocation = _nextRead;
            _leftRight.ToggleVersionAndWait();
            return _writeSecond;
        }

        public void Complete()
        {
            Monitor.Exit(_leftRight.WritersMutex);
        }

        public void Dispose()
        {
            Complete();
        }

    }

    class TokenLeftRight<T>
    {
        private LeftRight _leftRight;

        private readonly T _left;
        private readonly T _right;

        public TokenLeftRight (T left, T right)
        {
            _left = left;
            _right = right;
            _leftRight = new LeftRight();
        }

        public ReadToken<T> BeginRead()
        {
            WhereToRead localReadLocation = _leftRight.ReadLocation; // should be read-acquire
            int localVersionIndex = _leftRight.Arrive();
            var result = new ReadToken<T>(_leftRight, localVersionIndex, localReadLocation == WhereToRead.ReadOnLeft ? _left : _right);
            return result;
        }

        public WriteToken<T> BeginWrite()
        {
            var writeLeftFirst = new WriteToken<T>(_left, _right, WhereToRead.ReadOnLeft, _leftRight);
            var writeRightFirst = new WriteToken<T>(_right, _left, WhereToRead.ReadOnRight, _leftRight);
            var result = _leftRight.ReadLocation == WhereToRead.ReadOnLeft ? writeRightFirst : writeLeftFirst; 
            return result;
        }
    }




    enum WhereToRead
    {
        ReadOnLeft,
        ReadOnRight
    }

    class LeftRight
    {
        private IReadIndicator[] _readIndicator;
        private IWaitStrategy _waitStrategy;
        private int _versionIndex;

        public WhereToRead ReadLocation { get; set;}

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

    class LeftRightSynchronized<T>
    {
        private readonly T _left;
        private readonly T _right;
        private readonly LeftRight _leftRight;

        public LeftRightSynchronized(T left, T right, LeftRight leftRight)
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
            lock(_leftRight.WritersMutex)
            {
                if(_leftRight.ReadLocation == WhereToRead.ReadOnLeft)
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

    class Program
    {
        static void Main(string[] args)
        {
            var left = new Dictionary<string, int>();
            var right = new Dictionary<string, int>();
        
            var tokenLeftRight = new TokenLeftRight<Dictionary<string, int>>(left, right);

            using(var readToken = tokenLeftRight.BeginRead())
            {
                var instance = readToken.ReadInstance;
                bool x = instance.ContainsKey("hello");
            }

            using(var writeToken = tokenLeftRight.BeginWrite())
            {
                
                var firstInstance = writeToken.WriteFirst;
                firstInstance.Add("hello", 1);
                var secondInstance = writeToken.EndFirstWrite();
                secondInstance.Add("hello", 2);
            }


            var leftRightSync = new LeftRightSynchronized<Dictionary<string, int>>(left, right, new LeftRight(null));

            int total = leftRightSync.Read(i => 
            { int t = 0; foreach(var x in i.Values) { t += x;} return t;} );

            bool y = leftRightSync.Read(i => i.ContainsKey("hello"));
        
            leftRightSync.Write(i => i.Add("hello", 1));
        }
    }
}
