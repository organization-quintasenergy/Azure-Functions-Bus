using System;
using AFBus;
using AFBus.Tests.TestClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AFBus.Tests
{
    [TestClass]
    public class HandlersContainer_Tests
    {

        [TestMethod]
        public void HandlersContainer_IHandleTypesAreCorrectlyScanned()
        {
            var container = new HandlersContainer();

            Assert.IsTrue(container.messageHandlersDictionary[typeof(TestMessage)].Count == 2);

        }

        [TestMethod]
        public void HandlersContainer_IHandleTypesAreCorrectlyInvoked()
        {
            var container = new HandlersContainer();

            Assert.IsTrue(container.messageHandlersDictionary[typeof(TestMessage)].Count == 2);

            container.HandleAsync(new TestMessage(), null).Wait();

            Assert.IsTrue(InvocationCounter.Instance.Counter == 2);
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void HandlersContainer_HandlerNotFoundForThisMessage()
        {
            var container = new HandlersContainer();

            Assert.IsTrue(container.messageHandlersDictionary[typeof(TestMessage)].Count == 2);

            container.HandleAsync(new TestMessageHandler2(), null).Wait();
            
        }

        

    }
}
