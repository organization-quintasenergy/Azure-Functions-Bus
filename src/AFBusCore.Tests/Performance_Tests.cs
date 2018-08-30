using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AFBus.Tests.TestClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace AFBus.Tests
{

    [TestClass]
    public class Performance_Tests
    {
        readonly static string SERVICENAME = "FAKESERVICE";
        static HandlersContainer container = new HandlersContainer(SERVICENAME);

        readonly int NUMBER_OF_MESSAGES = 1000000;

                
        [TestMethod]
        public void Performance_StatelessHandler()
        {
            var messages = Enumerable.Range(0, NUMBER_OF_MESSAGES).Select(i => new TestMessage() { SomeData = i.ToString() });
                        
            var before = DateTime.Now;
                       
            Parallel.ForEach(messages, m =>
            {
                container.HandleAsync(m, null).Wait();

            });

            var after = DateTime.Now;

            var difference = after - before;

            Assert.IsTrue(difference.TotalMilliseconds<10000);

        }


    }
}
