using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AFBus.Tests.TestClasses;
using System.Diagnostics;

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

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = @"/c C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe clear all";
            process.StartInfo = startInfo;
            process.Start();

            process.WaitForExit();

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
            bus.Context = new AFBusMessageContext();

            bus.SendAsync(message, SERVICENAME).Wait();

            var stringMessage = QueueReader.ReadOneMessageFromQueue(SERVICENAME).Result;

            var finalMessageEnvelope = JsonConvert.DeserializeObject<AFBusMessageEnvelope>(stringMessage, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            });

            var finalMessage = JsonConvert.DeserializeObject<TestMessage>(finalMessageEnvelope.Body, new JsonSerializerSettings()
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
            bus.Context = new AFBusMessageContext();

            var before = DateTime.Now;
            var timeDelayed = new TimeSpan(0, 0, 5);
            bus.SendAsync(message, SERVICENAME, timeDelayed).Wait();

            string stringMessage = null;

            do
            {
                stringMessage = QueueReader.ReadOneMessageFromQueue(SERVICENAME).Result;
            }
            while (string.IsNullOrEmpty(stringMessage));

            var after = DateTime.Now;

            var finalMessageEnvelope = JsonConvert.DeserializeObject<AFBusMessageEnvelope>(stringMessage, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            });

            var finalMessage = JsonConvert.DeserializeObject<TestMessage>(finalMessageEnvelope.Body, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            });

            Assert.IsTrue(after-before> timeDelayed,"Delay failed");
        }
    }
}
