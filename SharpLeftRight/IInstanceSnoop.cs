namespace SharpLeftRight
{
    interface IInstanceSnoop
    {
        void BeginRead(long which);
        void EndRead(long which);
        void BeginWrite(long which);
        void EndWrite(long which);
    }

    class NullSnoop : IInstanceSnoop
    {
        public static readonly NullSnoop Instance = new NullSnoop(); 
        public void BeginRead(long which)
        {
        }

        public void BeginWrite(long which)
        {
        }

        public void EndRead(long which)
        {
        }

        public void EndWrite(long which)
        {
        }
    }
}
