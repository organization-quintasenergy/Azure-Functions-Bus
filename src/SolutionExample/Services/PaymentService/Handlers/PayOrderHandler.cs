using AFBus;
using Microsoft.Azure.WebJobs.Host;
using PaymentService.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Handlers
{
    public class PayOrderHandler : IHandle<PayOrder>
    {
        public async Task HandleAsync(IBus bus, PayOrder message, TraceWriter Log)
        {
            Log.Info("Order payed");

            await bus.SendAsync(new PayOrderResponse() { UserName = "pablo"}, message.ReplyTo);

        }
    }
}
