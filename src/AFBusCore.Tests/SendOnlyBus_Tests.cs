using System;
using AFBus.Tests.TestClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace AFBus.Tests
{
    [TestClass]
    public class SendOnlyBus_Tests
    {
        readonly string SERVICENAME = "FAKESERVICE";

        [TestMethod]
        public void SendOnlyBus_SendAsync_Nominal()
        {
            var id = Guid.NewGuid();

            var message = new TestMessage()
            {
                SomeData = id.ToString()
            };

            SendOnlyBus.SendAsync(message, SERVICENAME).Wait();

            var stringMessage = QueueReader.ReadOneMessageFromQueueAsync(SERVICENAME).Result;

            var finalMessageEnvelope = JsonConvert.DeserializeObject<AFBusMessageEnvelope>(stringMessage, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
            });

            var finalMessage = JsonConvert.DeserializeObject<TestMessage>(finalMessageEnvelope.Body, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
            });

            Assert.IsTrue(id.ToString() == finalMessage.SomeData);
        }

        [TestMethod]
        public void SendOnlyBus_SendAsync_DelayedMessage()
        {
            QueueReader.CleanQueueAsync(SERVICENAME).Wait();

            var message = new TestMessage()
            {
                SomeData = "delayed"
            };

            var serializer = new JSONSerializer();
            var publisher = new Mock<IPublishEvents>();
            IBus bus = new Bus(serializer, new AzureStorageQueueSendTransport(serializer), publisher.Object);

            var before = DateTime.Now;
            var timeDelayed = new TimeSpan(0, 0, 3);

            SendOnlyBus.SendAsync(message, SERVICENAME, timeDelayed).Wait();
            
            string stringMessage = null;

            do
            {
                stringMessage = QueueReader.ReadOneMessageFromQueueAsync(SERVICENAME).Result;
            }
            while (string.IsNullOrEmpty(stringMessage));

            var after = DateTime.Now;

            var finalMessageEnvelope = JsonConvert.DeserializeObject<AFBusMessageEnvelope>(stringMessage, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
            });

            var finalMessage = JsonConvert.DeserializeObject<TestMessage>(finalMessageEnvelope.Body, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
            });

            Assert.IsTrue(after - before > timeDelayed, "Delay failed");
        }
    }
}
