using System;
using AFUtils;
using AFUtils.IoC;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using ServiceA.Host.Handlers;
using ServiceA.Messages;

namespace ServiceA.Host
{
    public static class Function1
    {
        
        private static IFunctionContainer container = new FunctionContainer();
        
        [FunctionName("FunctionA")]
        public async static void Run([QueueTrigger("servicea", Connection = "")]string message, TraceWriter log)
        {
            MessageExample m = new MessageExample();
            m.SomeInfo = "Apple";

            var json = JsonConvert.SerializeObject(
                (ICommand)m,
                new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full
                });


            var command = JsonConvert.DeserializeObject(message, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormat=System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full
            });

            var messagetype = command.GetType();
            

            await container.InvokeAsync(command, null);

            //await function.InvokeAsync(command, log);

            log.Info($"C# Queue trigger function processed: {message}");
        }


    }
}
