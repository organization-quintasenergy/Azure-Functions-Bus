using AFBus;
using AFBus.Tests;
using AFBus.Tests.TestClasses;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AFBus.Tests
{
    [TestClass]
    public class BigMessage_Tests
    {
        readonly string SERVICENAME = "BIGMESSAGESERVICE";

       
        [TestMethod]
        public void BigMessage_GoingToFileInSend()
        {
            InvocationCounter.Instance.Reset();

            var container = new HandlersContainer();

            var id = Guid.NewGuid();

            var message = new BigMessage();
            message.Data = new string('*', 66000);
            
            SendOnlyBus.SendAsync(message, SERVICENAME).Wait();

            var stringMessage = QueueReader.ReadOneMessageFromQueue(SERVICENAME).Result;

            container.HandleAsync(stringMessage, null).Wait();

            Assert.IsTrue(InvocationCounter.Instance.Counter==1);
        }
    }

    public class BigMessage
    {
        public string Data { get; set; }
    }

    public class TestMessageHandler : IHandle<BigMessage>
    {
        public Task HandleAsync(IBus bus, BigMessage input, TraceWriter Log)
        {
            InvocationCounter.Instance.AddOne();

            return Task.CompletedTask;
        }
    }
}
