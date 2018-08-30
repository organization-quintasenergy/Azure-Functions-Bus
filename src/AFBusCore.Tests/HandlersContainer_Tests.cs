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
            var container = new HandlersContainer(SERVICENAME);

            Assert.IsTrue(container.messageHandlersDictionary[typeof(TestMessage)].Count == 2);

        }

        [TestMethod]
        public void HandlersContainer_IHandleTypesAreCorrectlyInvoked()
        {
            InvocationCounter.Instance.Reset();

            var container = new HandlersContainer(SERVICENAME);

            Assert.IsTrue(container.messageHandlersDictionary[typeof(TestMessage)].Count == 2);

            container.HandleAsync(new TestMessage(), null).Wait();

            Assert.IsTrue(InvocationCounter.Instance.Counter == 2);
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void HandlersContainer_HandlerNotFoundForThisMessage()
        {
            var container = new HandlersContainer(SERVICENAME);

            Assert.IsTrue(container.messageHandlersDictionary[typeof(TestMessage)].Count == 2);

            container.HandleAsync(new TestMessageHandler2(), null).Wait();
            
        }

        [TestMethod]
        public void HandlersContainer_HandleAsync_Sends_To_Delay_If_Some_Delay_Is_Left()
        {
            InvocationCounter.Instance.Reset();

            var container = new HandlersContainer(SERVICENAME);
           
            HandlersContainer.AddDependencyWithInstance<ISendMessages>(new AzureStorageQueueSendTransportShortMaxDelay(HandlersContainer.SolveDependency<ISerializeMessages>()));

           
            var message = new TestMessage()
            {
                SomeData = "delayed"
            };

            var serializer = HandlersContainer.SolveDependency<ISerializeMessages>();
            

            SendOnlyBus.SendAsync(message, SERVICENAME, TimeSpan.FromSeconds(10), serializer, new AzureStorageQueueSendTransportShortMaxDelay(serializer)).Wait();

            string stringMessage = null;

            do
            {
                stringMessage = QueueReader.ReadOneMessageFromQueueAsync(SERVICENAME).Result;
            }
            while (string.IsNullOrEmpty(stringMessage));

            container.HandleAsync(stringMessage, null).Wait();

            Assert.IsTrue(InvocationCounter.Instance.Counter == 0, "message not delayed");

            do
            {
                stringMessage = QueueReader.ReadOneMessageFromQueueAsync(SERVICENAME).Result;
            }
            while (string.IsNullOrEmpty(stringMessage));

            container.HandleAsync(stringMessage, null).Wait();


            Assert.IsTrue(InvocationCounter.Instance.Counter == 2, "message delayed more than once");
        }

        [TestMethod]
        public void HandlersContainer_HandleAsync_Sends_To_Delay_If_Some_Delay_Is_Left_2()
        {
            InvocationCounter.Instance.Reset();

            var container = new HandlersContainer(SERVICENAME);

            HandlersContainer.AddDependencyWithInstance<ISendMessages>(new AzureStorageQueueSendTransportShortMaxDelay(HandlersContainer.SolveDependency<ISerializeMessages>()));


            var message = new TestMessage()
            {
                SomeData = "delayed"
            };

            var serializer = HandlersContainer.SolveDependency<ISerializeMessages>();


            SendOnlyBus.SendAsync(message, SERVICENAME, TimeSpan.FromSeconds(15), serializer, new AzureStorageQueueSendTransportShortMaxDelay(serializer)).Wait();

            string stringMessage = null;

            do
            {
                stringMessage = QueueReader.ReadOneMessageFromQueueAsync(SERVICENAME).Result;
            }
            while (string.IsNullOrEmpty(stringMessage));

            container.HandleAsync(stringMessage, null).Wait();

            Assert.IsTrue(InvocationCounter.Instance.Counter == 0, "message not delayed");

            
            do
            {
                stringMessage = QueueReader.ReadOneMessageFromQueueAsync(SERVICENAME).Result;
            }
            while (string.IsNullOrEmpty(stringMessage));

            container.HandleAsync(stringMessage, null).Wait();

            Assert.IsTrue(InvocationCounter.Instance.Counter == 0, "message not delayed 2");

            do
            {
                stringMessage = QueueReader.ReadOneMessageFromQueueAsync(SERVICENAME).Result;
            }
            while (string.IsNullOrEmpty(stringMessage));

            container.HandleAsync(stringMessage, null).Wait();


            Assert.IsTrue(InvocationCounter.Instance.Counter == 2, "message delayed more than once");
        }
    }
}
