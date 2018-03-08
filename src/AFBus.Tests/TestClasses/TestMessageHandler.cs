using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFBus;

namespace AFBus.Tests.TestClasses
{
    public class TestMessageHandler : IHandle<TestMessage>
    {       

        public Task HandleAsync(IBus bus,TestMessage input, ITraceWriter Log)
        {
            InvocationCounter.Instance.AddOne();

            return Task.CompletedTask;
        }
    }
}
