using System;
using System.Threading.Tasks;
using AFBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace OrderSaga
{
    public static class Function1
    {
        const string SERVICENAME = "ordersaga";

        static HandlersContainer container = new HandlersContainer(SERVICENAME);

        [FunctionName("OrderSagaEndpointFunction")]
        public static async Task Run([QueueTrigger(SERVICENAME)]string orderSagaMessage, TraceWriter log)
        {
            log.Info($"OrderSagaEndpointFunction message received: {orderSagaMessage}");

            await container.HandleAsync(orderSagaMessage, log);
        }
    }
}
