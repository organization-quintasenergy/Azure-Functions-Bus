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
           /* MessageExample m = new MessageExample();
            m.SomeInfo = "Apple";

            var json = JsonConvert.SerializeObject(
                m,
                new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full
                });*/


            var command = JsonConvert.DeserializeObject(message, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormat=System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            });

            var messagetype = command.GetType();
            

            await container.InvokeAsync(command, new AFTraceWritter(log));
                       

            log.Info($"C# Queue trigger function processed: {message}");
        }


    }
}
