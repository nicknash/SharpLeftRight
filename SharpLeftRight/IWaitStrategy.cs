namespace SharpLeftRight
{
    public interface IWaitStrategy
    {
        void WaitWhileOccupied(IReadIndicator readIndicator);
    }
}