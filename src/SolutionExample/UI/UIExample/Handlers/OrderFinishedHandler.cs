using AFBus;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using OrderSaga.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UIExample.Handlers
{
    public class OrderFinishedHandler : IHandle<OrderFinished>
    {
        IHubContext<Events> hub;


        public OrderFinishedHandler(IHubContext<Events> hub)
        {
            this.hub = hub;
        }

        public async Task HandleAsync(IBus bus, OrderFinished message, ILogger Log)
        {
            await hub.Clients.All.SendAsync("Send", message.UserName);
            
        }
    }
}
