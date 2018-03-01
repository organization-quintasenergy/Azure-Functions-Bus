using System;
using AFUtils.Tests.TestClasses;
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
            var message = new TestMessage()
            {
                SomeData = "asdf"
            };

            SendOnlyBus.SendAsync(message, SERVICENAME).Wait();

            var stringMessage = QueueReader.ReadFromQueue(SERVICENAME).Result;

            var finalMessage = JsonConvert.DeserializeObject<TestMessage>(stringMessage, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            });
        }
    }
}
