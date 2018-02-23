using System.Collections.Generic;
using RelaSharp;

namespace SharpLeftRight.Tests
{
    class InstanceSnoop : IInstanceSnoop
    {
        private IRelaEngine RE = RelaEngine.RE;

        private HashSet<long> _reading = new HashSet<long>();
        private HashSet<long> _writing = new HashSet<long>();

        public void BeginRead(long which)
        {
            RE.MaybeSwitch();
            RE.Assert(!_writing.Contains(which), $"Write in progress during read at {which}");
            _reading.Add(which);
        }

        public void EndRead(long which)
        {
            RE.MaybeSwitch();
            _reading.Remove(which);
            RE.Assert(!_writing.Contains(which), $"Write in progress during read at {which}");
        }

        public void BeginWrite(long which)
        {
            RE.MaybeSwitch();
            RE.Assert(!_reading.Contains(which), $"Read in progress during write at {which}");
            RE.Assert(!_writing.Contains(which), $"Write in progress during write at {which}");
            _writing.Add(which);
        }

        public void EndWrite(long which)
        {
            RE.MaybeSwitch();
            RE.Assert(!_reading.Contains(which), $"Write in progress during read at {which}");
            _writing.Remove(which);
        }
    }
}
