using System;
using System.Threading.Tasks;
using AFBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace PaymentService
{
    public static class CommandsReceiverFunction
    {
        static HandlersContainer container = new HandlersContainer();

        [FunctionName("PaymentServiceEndpointFunction")]
        public static async Task Run([QueueTrigger("paymentservice", Connection = "")]string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");

            await container.HandleAsync(myQueueItem, new AFTraceWriter(log));
        }
    }
}
