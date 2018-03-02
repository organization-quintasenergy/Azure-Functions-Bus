using System;
using System.Threading.Tasks;
using AFBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using ServiceA.Host.Handlers;
using ServiceA.Messages;

namespace ServiceA.Host
{
    public static class ServiceAFunction
    {
        
        private static IFunctionContainer container = new FunctionContainer();


        [FunctionName("ServiceAEndpoint2")]
        public async static Task Run([QueueTrigger("servicea", Connection = "")]string message, TraceWriter log)
        {                      

            await container.InvokeAsync(message, new AFTraceWritter(log));
                       

            log.Info($"C# Queue trigger function processed: {message}");
        }


    }
}
