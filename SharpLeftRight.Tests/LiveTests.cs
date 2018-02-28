using System;
using System.Collections.Generic;
using Xunit;
using RelaSharp;
using SharpLeftRight;
using System.Linq;
using System.Threading;

namespace SharpLeftRight.Tests
{
    public class LiveTests
    {  
        [Fact]
        public void Read_AfterWrites_IsCorrect()
        {
            RelaEngine.Mode = EngineMode.Live;
            var leftRightList = LeftRightBuilder.Build(new List<int>(), new List<int>());
            leftRightList.Write(d => d.Add(1));
            leftRightList.Write(d => d.Add(2));
            leftRightList.Write(d => d.Add(3));

            var equal = leftRightList.Read(d => d.SequenceEqual(new[]{1,2,3}));

            Xunit.Assert.True(equal);
        }

        [Fact]
        public void Read_DuringAndAfterWriting_FinalReadIsCorrect()
        {
            var barrier = new Barrier(4);
            var leftRightList = LeftRightBuilder.Build(new List<int>(), new List<int>());            
            int total = 0;
            bool writeFinished = false;
            WaitCallback reader = _ => 
                { 
                    barrier.SignalAndWait();
                    while(!writeFinished)
                    {
                        total = leftRightList.Read<int>(l => { int t = 0; l.ForEach(v => { t += v; }); return t; });
                    }
                    total = leftRightList.Read<int>(l => { int t = 0; l.ForEach(v => { t += v; }); return t; });
                    barrier.SignalAndWait();
                };
            int maxWritten = 1000;
            WaitCallback writer = _ => 
                {
                    barrier.SignalAndWait();
                    for(int i = 0; i <= maxWritten; ++i)
                    {
                        leftRightList.Write(l => l.Add(i));
                    }
                    writeFinished = true;
                    barrier.SignalAndWait();
                };
            ThreadPool.QueueUserWorkItem(reader);
            ThreadPool.QueueUserWorkItem(reader);
            ThreadPool.QueueUserWorkItem(writer);
            barrier.SignalAndWait();
            barrier.SignalAndWait();
            Xunit.Assert.Equal(maxWritten * (maxWritten + 1) / 2, total);
        }
        
    }
}