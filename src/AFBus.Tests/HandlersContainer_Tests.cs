using System;
using System.Threading.Tasks;
using AFBus;
using AFBus.Tests.TestClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace AFBus.Tests
{
    [TestClass]
    public class HandlersContainer_Tests
    {
        readonly static string SERVICENAME = "FAKESERVICE";

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

        [TestMethod]
        public void HandlersContainer_IHandleTypesAreCorrectlyInvokedWithDelay()
        {
            int callCount = 0;
            var container = new HandlersContainer();
            var transportMock = new Mock<ISendMessages>();
            transportMock.Setup(t => t.MaxDelay()).Returns(new TimeSpan(0,0,1));
            transportMock.Setup(t => t.SendMessageAsync(It.IsAny<TestMessage>(), It.IsAny<string>(), It.IsAny<AFBusMessageContext>()))
                .Callback<Func<int>>(f => callCount++);

            HandlersContainer.AddDependencyWithInstance<ISendMessages>(transportMock.Object);

            var messageContext = new AFBusMessageContext()
            {
                Destination = SERVICENAME,
                DelayedTime = new TimeSpan(0, 0, 5),
                MessageID = Guid.NewGuid(),
                TransactionID = Guid.NewGuid()
                
            };

            var message = new TestMessage()
            {
                SomeData = "delayed"
            };

            var serializer = HandlersContainer.SolveDependency<ISerializeMessages>();

            var finalMessage = new AFBusMessageEnvelope()
            {
                Context = messageContext,
                Body = serializer.Serialize(message)
            };

            container.HandleAsync(serializer.Serialize(finalMessage), null).Wait();

            transportMock.Verify(m => m.SendMessageAsync(It.IsAny<TestMessage>(), It.IsAny<string>(), It.IsAny<AFBusMessageContext>()), Times.AtLeastOnce());
        }

    }
}
