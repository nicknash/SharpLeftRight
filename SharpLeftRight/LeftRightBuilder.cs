namespace SharpLeftRight
{
    public class LeftRightBuilder
    {
        public static LeftRightSynchronised<T> Build<T>(T left, T right)
        {
            var waitStrategy = new YieldWaitStrategy();
            var readIndicators = new[]{BuildReadIndicator(), BuildReadIndicator()};
            var leftRightSync = new LeftRightSynchronised<T>(left, right, new LeftRight(waitStrategy, readIndicators));
            return leftRightSync;
        }

        public static LeftRightSynchronised<T> Build<T>(T left, T right, IReadIndicator leftReadIndicator, IReadIndicator rightReadIndicator)
        {
            var waitStrategy = new YieldWaitStrategy();
            var readIndicators = new[]{leftReadIndicator, rightReadIndicator};
            var leftRightSync = new LeftRightSynchronised<T>(left, right, new LeftRight(waitStrategy, readIndicators));
            return leftRightSync;
        }

        public static LeftRightSynchronised<T> Build<T>(T left, T right, IWaitStrategy waitStrategy)
        {
            var readIndicators = new[]{BuildReadIndicator(), BuildReadIndicator() };
            var leftRightSync = new LeftRightSynchronised<T>(left, right, new LeftRight(waitStrategy, readIndicators));
            return leftRightSync;
        }

        public static LeftRightSynchronised<T> Build<T>(T left, T right, IReadIndicator leftReadIndicator, IReadIndicator rightReadIndicator, IWaitStrategy waitStrategy)
        {
            var readIndicators = new[]{leftReadIndicator, rightReadIndicator};
            var leftRightSync = new LeftRightSynchronised<T>(left, right, new LeftRight(waitStrategy, readIndicators));
            return leftRightSync;
        }

        private static HashedReadIndicator BuildReadIndicator() => new HashedReadIndicator(8, 7);
  }
}
