using System;
using AFBus.Tests.TestClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

            var stringMessage = QueueReader.ReadFromQueue(SERVICENAME).Result;

            var finalMessage = JsonConvert.DeserializeObject<TestMessage>(stringMessage, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            });

            Assert.IsTrue(id.ToString() == finalMessage.SomeData);
        }

        [TestMethod]
        public void SendOnlyBus_SendAsync_DelayedMessage()
        {
            var message = new TestMessage()
            {
                SomeData = "delayed"
            };

            var serializer = new JSONSerializer();
            IBus bus = new Bus(serializer, new AzureStorageQueueSendTransport(serializer));

            var before = DateTime.Now;
            var timeDelayed = new TimeSpan(0, 0, 3);

            SendOnlyBus.SendAsync(message, SERVICENAME, timeDelayed).Wait();
            
            string stringMessage = null;

            do
            {
                stringMessage = QueueReader.ReadFromQueue(SERVICENAME).Result;
            }
            while (string.IsNullOrEmpty(stringMessage));

            var after = DateTime.Now;
            
            var finalMessage = JsonConvert.DeserializeObject<TestMessage>(stringMessage, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            });

            Assert.IsTrue(after - before > timeDelayed, "Delay failed");
        }
    }
}
