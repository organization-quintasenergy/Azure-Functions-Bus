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
        public async Task HandleAsync(IBus bus, ShipOrder message, TraceWriter Log)
        {
            Log.Info("order shipped");
                        
            await bus.SendAsync(new ShipOrderResponse() { UserName = message.UserName }, message.ReplyTo);

            
        }
    }
}
