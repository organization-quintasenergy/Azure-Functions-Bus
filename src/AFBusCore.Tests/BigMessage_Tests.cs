using AFBus;
using AFBus.Tests;
using AFBus.Tests.TestClasses;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace AFBus.Tests
{
    [TestClass]
    public class BigMessage_Tests
    {
        public static string SERVICENAME = "BIGMESSAGESERVICE";

       
        [TestMethod]
        public void BigMessage_Going_To_File_In_Send()
        {
            InvocationCounter.Instance.Reset();

            var container = new HandlersContainer();

            var message = new BigMessage();
            message.Data = new string('*', 66000);
            
            SendOnlyBus.SendAsync(message, SERVICENAME).Wait();

            var stringMessage = QueueReader.ReadOneMessageFromQueue(SERVICENAME).Result;

            container.HandleAsync(stringMessage, null).Wait();

            Assert.IsTrue(InvocationCounter.Instance.Counter==1);
        }

        [TestMethod]
        public void BigMessage_Bug_Small_After_Big_Mixes_Message_In_File_Flag()
        {
            InvocationCounter.Instance.Reset();

            var container = new HandlersContainer();
           
            var message = new BigMessage2();
            message.Data = new string('*', 66000);

            SendOnlyBus.SendAsync(message, SERVICENAME).Wait();

            var stringMessage = QueueReader.ReadOneMessageFromQueue(SERVICENAME).Result;

            container.HandleAsync(stringMessage, null).Wait();

            Assert.IsTrue(BlobReader.ListFiles().Result.Count() == 0);
        }       
    
    }

    public class BigMessage
    {
        public string Data { get; set; }
    }

    public class BigMessage2
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

    public class TestMessageHandler2 : IHandle<BigMessage2>
    {
        public Task HandleAsync(IBus bus, BigMessage2 input, TraceWriter Log)
        {
            bus.SendAsync(new BigMessage(), BigMessage_Tests.SERVICENAME).Wait();

            return Task.CompletedTask;
        }
    }
}
