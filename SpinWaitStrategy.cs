namespace SharpLeftRight
{
    class SpinWaitStrategy : IWaitStrategy
    {
        public void WaitWhileOccupied(IReadIndicator readIndicator)
        {
            while(readIndicator.IsOccupied) ;
        }
    }
}