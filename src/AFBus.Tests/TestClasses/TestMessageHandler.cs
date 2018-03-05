using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFBus;

namespace AFBus.Tests.TestClasses
{
    public class TestMessageHandler : IFunction<TestMessage>
    {       

        public Task InvokeAsync(IBus bus,TestMessage input, ITraceWriter Log)
        {
            InvocationCounter.Instance.AddOne();

            return Task.CompletedTask;
        }
    }
}
