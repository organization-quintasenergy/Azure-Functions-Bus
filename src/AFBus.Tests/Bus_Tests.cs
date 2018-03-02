using System;
using AFUtils.Tests.TestClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AFBus.Tests
{
    [TestClass]
    public class Bus_Tests
    {
        readonly static string SERVICENAME = "FAKESERVICE";

        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            QueueReader.CleanQueue(SERVICENAME).Wait();
        }

        [TestMethod]
        public void Bus_SendAsync_Nominal()
        {
            var id = Guid.NewGuid();

            var message = new TestMessage()
            {
                SomeData = id.ToString()
            };

            var serializer = new JSONSerializer();
            IBus bus = new Bus(serializer, new AzureStorageQueueSendTransport(serializer));
            bus.SendAsync(message, SERVICENAME).Wait();

            var stringMessage = QueueReader.ReadFromQueue(SERVICENAME).Result;

            var finalMessage = JsonConvert.DeserializeObject<TestMessage>(stringMessage, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            });

            Assert.IsTrue(id.ToString() == finalMessage.SomeData);

        }

        [TestMethod]
        public void Bus_SendAsync_DelayedMessage()
        {
            var message = new TestMessage()
            {
                SomeData = "delayed"
            };

            var serializer = new JSONSerializer();
            IBus bus = new Bus(serializer, new AzureStorageQueueSendTransport(serializer));

            var before = DateTime.Now;
            var timeDelayed = new TimeSpan(0, 0, 3);
            bus.SendAsync(message, SERVICENAME, timeDelayed).Wait();

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

            Assert.IsTrue(after-before> timeDelayed,"Delay failed");
        }
    }
}
