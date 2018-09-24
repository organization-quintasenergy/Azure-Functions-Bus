using System;
using System.Threading.Tasks;
using AFBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace OrderSaga
{
    public static class Function1
    {
        const string SERVICENAME = "ordersaga";

        static HandlersContainer container = new HandlersContainer(SERVICENAME);

        [FunctionName("OrderSagaEndpointFunction")]
        public static async Task Run([QueueTrigger(SERVICENAME)]string orderSagaMessage, ILogger log)
        {
            log.LogInformation($"OrderSagaEndpointFunction message received: {orderSagaMessage}");

            await container.HandleAsync(orderSagaMessage, log);
        }
    }
}
