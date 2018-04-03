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
        IPaymentsRepository repository;

        public PayOrderHandler(IPaymentsRepository rep)
        {
            this.repository = rep;
        }

        public async Task HandleAsync(IBus bus, PayOrder message, TraceWriter Log)
        {
            Log.Info("Order payed");            

            repository.AddOrderPayed(new OrderPayed { User = message.UserName });

            await bus.SendAsync(new PayOrderResponse() { UserName = message.UserName}, message.ReplyTo);

        }
    }
}
