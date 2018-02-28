using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFUtils.Tests.TestClasses
{
    /// <summary>
    /// Util for counting the number of invocations
    /// </summary>
    public class InvocationCounter
    {
        private static InvocationCounter singleton = new InvocationCounter();

        private InvocationCounter()
        {

        }

        public static InvocationCounter Instance { get { return singleton; } }
        
        private int counter = 0;

        public int Counter { get { return counter; } }

        public void AddOne()
        {
            counter++;
        }




    }
}
