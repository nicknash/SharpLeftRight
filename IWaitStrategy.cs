namespace SharpLeftRight
{
    interface IWaitStrategy
    {
        void WaitWhileOccupied(IReadIndicator readIndicator);
    }
}