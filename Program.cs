using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SharpLeftRight
{
    class Program
    {
        static void Main(string[] args)
        {
            var left = new Dictionary<string, int>();
            var right = new Dictionary<string, int>();

            var waitStrategy = new SpinWaitStrategy();
            var readIndicators = new[]{new HashedReadIndicator(10, 7), new HashedReadIndicator(10, 7) };
            var leftRightSync = new LeftRightSynchronised<Dictionary<string, int>>(left, right, new LeftRight(waitStrategy, readIndicators));

            int total = leftRightSync.Read(i => 
            { int t = 0; foreach(var x in i.Values) { t += x;} return t;} );

            bool y = leftRightSync.Read(i => i.ContainsKey("hello"));
        
            leftRightSync.Write(i => i.Add("hello", 1));

            Console.WriteLine($"{total}, {leftRightSync.Read(d => d["hello"])}");
        }
    }
}
