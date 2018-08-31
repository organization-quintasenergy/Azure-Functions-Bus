using System;
using System.Threading.Tasks;
using AFBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace ShippingService
{
    public static class ShippingService
    {
        const string SERVICENAME = "shippingservice";
        static HandlersContainer container = new HandlersContainer(SERVICENAME);

        static ShippingService()
        {
            HandlersContainer.AddDependency<IShippingRepository, InMemoryShippingRepository>();
        }


        [FunctionName("ShippingServiceEndpointFunction")]
        public static async Task Run([QueueTrigger("shippingservice")]string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");

            await container.HandleAsync(myQueueItem, log);
        }
    }
}
