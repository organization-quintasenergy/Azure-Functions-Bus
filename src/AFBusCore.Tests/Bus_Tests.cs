using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AFBus.Tests.TestClasses;
using System.Diagnostics;
using Moq;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.EventHubs;

namespace AFBus.Tests
{
    [TestClass]
    public class Bus_Tests
    {
        readonly static string SERVICENAME = "FAKESERVICE";
        readonly static string TOPICNAME = "FAKETOPIC";

        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            QueueReader.CleanQueueAsync(SERVICENAME).Wait();

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
            QueueReader.CleanQueueAsync(SERVICENAME).Wait();

            var id = Guid.NewGuid();

            var message = new TestMessage()
            {
                SomeData = id.ToString()
            };

            var serializer = new JSONSerializer();
            var publisher = new Mock<IPublishEvents>();
            IBus bus = new Bus(serializer, new AzureStorageQueueSendTransport(serializer), publisher.Object);
            bus.Context = new AFBusMessageContext();

            bus.SendAsync(message, SERVICENAME).Wait();

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
        public void Bus_SendAsync_DelayedMessage()
        {
            QueueReader.CleanQueueAsync(SERVICENAME).Wait();

            var message = new TestMessage()
            {
                SomeData = "delayed"
            };

            var serializer = new JSONSerializer();
            var publisher = new Mock<IPublishEvents>();
            IBus bus = new Bus(serializer, new AzureStorageQueueSendTransport(serializer), publisher.Object);
            bus.Context = new AFBusMessageContext();

            var before = DateTime.Now;
            var timeDelayed = new TimeSpan(0, 0, 5);
            bus.SendAsync(message, SERVICENAME, timeDelayed).Wait();

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

            Assert.IsTrue(after-before> timeDelayed,"Delay failed");
        }

        [TestMethod]
        public void Bus_PublishAsync_EventHub_Nominal()
        {
           
            var id = Guid.NewGuid();

            bool testOk = false;

            var message = new TestMessage()
            {
                SomeData = id.ToString()
            };

            var serializer = new JSONSerializer();
            var publisher = new AzureEventHubPublishTransport(serializer);
            IBus bus = new Bus(serializer, new AzureStorageQueueSendTransport(serializer), publisher);
            bus.Context = new AFBusMessageContext();

            bus.PublishAsync(message, TOPICNAME).Wait();

            var eventProcessorHost = new EventProcessorHost(TOPICNAME, PartitionReceiver.DefaultConsumerGroupName, SettingsUtil.GetSettings<string>(SETTINGS.AZURE_EVENTHUB), SettingsUtil.GetSettings<string>(SETTINGS.AZURE_STORAGE), "eventhubcontainer");

            // Registers the Event Processor Host and starts receiving messages
            var readingTask = eventProcessorHost.RegisterEventProcessorFactoryAsync(new AzureStreamProcessorFactory(stringMessage =>

            {
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

                testOk = testOk || (id.ToString() == finalMessage.SomeData);
            }));

            

            Task.Delay(5000).Wait();
            

            // Disposes of the Event Processor Host
            eventProcessorHost.UnregisterEventProcessorAsync().Wait();

            Assert.IsTrue(testOk);

        }
    }
}
