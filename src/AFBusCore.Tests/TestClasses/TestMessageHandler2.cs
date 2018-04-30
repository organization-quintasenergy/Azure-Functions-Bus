using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFBus;
using Microsoft.Azure.WebJobs.Host;

namespace AFBus.Tests.TestClasses
{
    public class TestMessageHandler2 : IHandle<TestMessage>
    {
        public Task HandleAsync(IBus bus, TestMessage input, TraceWriter Log)
        {
            InvocationCounter.Instance.AddOne();

            return Task.CompletedTask;
        }
    }
}
