using System;
using System.Threading.Tasks;
using AFBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace ShippingService
{
    public static class ShippingService
    {
        static HandlersContainer container = new HandlersContainer();

        [FunctionName("ShippingServiceEndpointFunction")]
        public static async Task Run([QueueTrigger("shippingservice", Connection = "")]string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");

            await container.HandleAsync(myQueueItem, log);
        }
    }
}
