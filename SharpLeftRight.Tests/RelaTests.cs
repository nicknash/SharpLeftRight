using System;
using System.Collections.Generic;
using Xunit;
using RelaSharp;
using SharpLeftRight;

namespace SharpLeftRight.Tests
{
    public class RelaTests
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
                ThreadEntries = new Action[]{ReadThread,ReadThread,WriteThread,WriteThread};
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


        [Fact]
        public void RelaSnoopTest()
        { 
            RelaEngine.Mode = EngineMode.Test;
            TestRunner.TR.RunTest(() => new LeftRightSnoopTest());
            bool failed = TestRunner.TR.TestFailed;
            if(failed)
            {
                TestRunner.TR.DumpExecutionLog(Console.Out);
            }
            Xunit.Assert.False(failed);
        }
    
   
    }
}
