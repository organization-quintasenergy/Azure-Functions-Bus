using System;
using System.Threading.Tasks;
using AFBus;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using ServiceA.Messages;

namespace ServiceA.Host
{
    public static class ScheduledFunction
    {
        //private static IFunctionContainer container = new FunctionContainer();

        [FunctionName("ScheduledFunction")]
        public static  Task Run([TimerTrigger("*/15 * * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            SendOnlyBus.SendAsync(new MessageExample(),"servicea").Wait();

            return Task.CompletedTask;
        }
    }
}
