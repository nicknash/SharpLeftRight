using System;
using Xunit;
using RelaSharp;
using SharpLeftRight;

namespace SharpLeftRight.Tests
{
    public class RelaTests
    { 

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
