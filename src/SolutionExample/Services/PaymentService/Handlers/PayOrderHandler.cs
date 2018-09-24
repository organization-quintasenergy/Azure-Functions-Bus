using AFBus;
using Microsoft.Extensions.Logging;
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

        public async Task HandleAsync(IBus bus, PayOrder message, ILogger Log)
        {
            Log.LogInformation("Order payed");            

            repository.AddOrderPayed(new OrderPayed { User = message.UserName });

            await bus.ReplyAsync(new PayOrderResponse() { UserName = message.UserName});

        }
    }
}
