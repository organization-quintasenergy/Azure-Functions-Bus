using AFBus;
using Microsoft.Azure.WebJobs.Host;
using OrderSaga.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UIExample.Handlers
{
    public class OrderFinishedHandler : IHandle<OrderFinished>
    {
        public Task HandleAsync(IBus bus, OrderFinished message, TraceWriter Log)
        {
            return Task.CompletedTask;
        }
    }
}
