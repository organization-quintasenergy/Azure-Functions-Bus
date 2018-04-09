using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;

namespace AFBus.Tests.TestClasses.DI
{
    public class DIMessageHandler : IHandle<DIMessage>
    {
        public DIMessageHandler(IUoWTest uow)
        {

        }

        public Task HandleAsync(IBus bus, DIMessage message, TraceWriter Log)
        {
            return Task.CompletedTask;
        }
    }
}
