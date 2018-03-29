using System;
using System.Threading.Tasks;
using AFBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace OrderSaga
{
    public static class Function1
    {
        static HandlersContainer container = new HandlersContainer();

        [FunctionName("OrderSagaEndpointFunction")]
        public static async Task Run([QueueTrigger("ordersaga")]string orderSagaMessage, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {orderSagaMessage}");

            await container.HandleAsync(orderSagaMessage, log);
        }
    }
}
