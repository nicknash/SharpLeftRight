using System.Threading;

namespace SharpLeftRight
{
    class YieldWaitStrategy : IWaitStrategy
    {
        public void WaitWhileOccupied(IReadIndicator readIndicator)
        {
            while(readIndicator.IsOccupied) 
            {
                Thread.Yield();
            }
        }
    }
}