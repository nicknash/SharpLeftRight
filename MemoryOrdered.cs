using System.Threading;

class MemoryOrdered
{
    private long _value;
    public MemoryOrdered(long initialValue)
    {
        _value = initialValue;
    }

    public long ReadUnordered => _value;

    public long ReadSeqCst
    {
        get
        {
            Interlocked.Read(ref _value);
            return _value;
        }
    }
    public void WriteUnordered(long newValue)
    {
        _value = newValue;
    }

    public void WriteSeqCst(long newValue)
    {
        Interlocked.Exchange(ref _value, newValue);
    }
}