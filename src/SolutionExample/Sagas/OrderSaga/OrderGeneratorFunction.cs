using System;
using System.Threading.Tasks;
using AFBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using OrderSaga.Messages;

namespace OrderSaga
{
    public static class OrderGeneratorFunction
    {
        static bool finished = false;

        [FunctionName("OrderGeneratorFunction")]
        public static async Task Run([TimerTrigger("*/15 * * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            /*log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            if (finished)
                return;

            await SendOnlyBus.SendAsync(new CartItemAdded() { UserName = "pablo", ProductName = "raspberry pi" }, "ordersaga");


            Random rnd = new Random();

            if (rnd.Next(10) % 5 == 0)
            {
                finished = true;
                await SendOnlyBus.SendAsync(new ProcessOrder() { UserName = "pablo" }, "ordersaga");
                                
            }*/
        
        }
    }
}
