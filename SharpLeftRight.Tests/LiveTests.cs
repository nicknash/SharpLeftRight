using System;
using System.Collections.Generic;
using Xunit;
using RelaSharp;
using SharpLeftRight;
using System.Linq;

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
    }
}