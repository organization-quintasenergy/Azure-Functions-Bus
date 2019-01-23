using AFBus;
using AFBus.Tests.TestClasses;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AFBus.Tests.TestClasses
{
    public interface IFakeInterface
    {

    }

    public class ClassWithTwoHandlersMessage1
    {

    }

    public class ClassWithTwoHandlersMessage2
    {

    }

    public class ClassWithTwoHandlers : IHandle<ClassWithTwoHandlersMessage1>, IFakeInterface, IHandle<ClassWithTwoHandlersMessage2>
    {
        public Task HandleAsync(IBus bus, ClassWithTwoHandlersMessage1 message, ILogger log)
        {
            InvocationCounter.Instance.AddOne();

            return Task.CompletedTask;
        }

        public Task HandleAsync(IBus bus, ClassWithTwoHandlersMessage2 message, ILogger log)
        {
            InvocationCounter.Instance.AddOne();

            return Task.CompletedTask;
        }

       
    }
}
