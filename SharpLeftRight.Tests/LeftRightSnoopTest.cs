using System;
using System.Collections.Generic;
using RelaSharp;

namespace SharpLeftRight.Tests
{
    class LeftRightSnoopTest : IRelaTest
    {
        private static IRelaEngine RE = RelaEngine.RE;

        public IReadOnlyList<Action> ThreadEntries { get; }
        private LeftRightSynchronised<Dictionary<int, string>> _leftRightDictionary;
        private int _numWrites = 5;

        public LeftRightSnoopTest()
        {
            RelaEngine.Mode = EngineMode.Test;
            ThreadEntries = new Action[] { ReadThread, ReadThread, WriteThread, WriteThread };
        }

        public void OnBegin()
        {
            _leftRightDictionary = LeftRightBuilder.Build(new Dictionary<int, string>(), new Dictionary<int, string>());
            _leftRightDictionary.SetSnoop(new InstanceSnoop());
        }

        public void OnFinished()
        {
        }

        public void ReadThread()
        {
            for (int i = 0; i < 5; ++i)
            {
                string message = null;
                bool read = _leftRightDictionary.Read(d => d.TryGetValue(i, out message));
            }
        }

        public void WriteThread()
        {
            for (int i = 0; i < _numWrites; ++i)
            {
                _leftRightDictionary.Write(d => d[i] = $"Wrote this: {i}");
            }
        }
    }
}
