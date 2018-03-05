using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace WorkflowA.Host
{
    public static class WorkflowA
    {
            
        [FunctionName("WorkflowA.Host")]
        public static void Run([QueueTrigger("workflowa", Connection = "")]string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}
