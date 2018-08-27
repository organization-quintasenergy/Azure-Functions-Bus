using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    public class AzureEventHubEventProcessor : IEventProcessor
    {

        Action<string> messageProcessor;

        public AzureEventHubEventProcessor(Action<string> messageProcessor)
        {
            this.messageProcessor = messageProcessor;
        }

        public Task ProcessErrorAsync(PartitionContext context, Exception error)
        {
            throw new NotImplementedException();
        }

        async Task IEventProcessor.CloseAsync(PartitionContext context, CloseReason reason)
        {
           
            if (reason == CloseReason.Shutdown)
            {
                await context.CheckpointAsync();
            }
        }

        Task IEventProcessor.OpenAsync(PartitionContext context)
        {
            return Task.CompletedTask;
        }

        async Task IEventProcessor.ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (EventData eventData in messages)
            {
                string data = Encoding.UTF8.GetString(eventData.Body.Array);

                messageProcessor.Invoke(data);

                await context.CheckpointAsync();

                Console.WriteLine(string.Format("Message received.  Partition: '{0}', Data: '{1}'",
                    context.PartitionId, data));
            }

           
        }
    }


    public class AzureStreamProcessorFactory : IEventProcessorFactory
    {
        Action<string> messageProcessor;

        public AzureStreamProcessorFactory(Action<string> messageProcessor)
        {
            this.messageProcessor = messageProcessor;
        }

        IEventProcessor IEventProcessorFactory.CreateEventProcessor(PartitionContext context)
        {
            return new AzureEventHubEventProcessor(messageProcessor);
        }
    }

}
