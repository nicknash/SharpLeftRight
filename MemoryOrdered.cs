using System.Threading;

class MemoryOrdered
{
    private long _value;
    public MemoryOrdered(long initialValue)
    {
        _value = initialValue;
    }

    public long ReadSeqCst
    {
        get
        {
            Interlocked.Read(ref _value);
            return _value;
        }
    }
    public void WriteSeqCst(long newValue)
    {
        Interlocked.Exchange(ref _value, newValue);
    }
}