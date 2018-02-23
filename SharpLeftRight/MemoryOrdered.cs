using System.Threading;
using RelaSharp.CLR;

class MemoryOrdered
{
    private CLRAtomicLong _value;
    public MemoryOrdered(long initialValue)
    {
        RVolatile.Write(ref _value, initialValue);
    }

    public long ReadUnordered => RUnordered.Read(ref _value);

    public long ReadSeqCst
    {
        get
        {
            return RInterlocked.Read(ref _value);
        }
    }
    public void WriteUnordered(long newValue)
    {
        RUnordered.Write(ref _value, newValue);
    }

    public void WriteSeqCst(long newValue)
    {
        RInterlocked.Exchange(ref _value, newValue);
    }
}