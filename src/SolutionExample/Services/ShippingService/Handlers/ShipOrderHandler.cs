using AFBus;
using Microsoft.Azure.WebJobs.Host;
using ShippingService.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShippingService.Handlers
{
    public class ShipOrderHandler : IHandle<ShipOrder>
    {

        IShippingRepository rep;

        public ShipOrderHandler(IShippingRepository rep)
        {
            this.rep = rep;
        }

        public async Task HandleAsync(IBus bus, ShipOrder message, TraceWriter Log)
        {
            Log.Info("order shipped");

            rep.AddOrderShipped(new OrderShipped { User = message.UserName });

            await bus.ReplyAsync(new ShipOrderResponse() { UserName = message.UserName });

            
        }
    }
}
