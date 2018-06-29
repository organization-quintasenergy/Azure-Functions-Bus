using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace AFBus
{
    public class Bus : IBus
    {
        ISerializeMessages serializer; 
        ISendMessages sender;
        IPublishEvents publisher;

        internal Bus(ISerializeMessages serializer, ISendMessages sender, IPublishEvents publisher)
        {
            this.serializer = serializer;
            this.sender = sender;
            this.publisher = publisher;
        }

        ISerializeMessages IBus.Serializer => this.serializer;

        public AFBusMessageContext Context { get; set; }

        /// <summary>
        /// Sends a message to a queue named like the service.
        /// </summary>
        public async Task SendAsync<T>(T input, string serviceName, TimeSpan? initialVisibilityDelay = null) where T : class
        {
            var newContext = new AFBusMessageContext();

            newContext.MessageID = Guid.NewGuid();
            newContext.TransactionID = Context.TransactionID ?? Guid.NewGuid();
            newContext.BodyType = typeof(T).AssemblyQualifiedName;
            newContext.BodyInFile = false;
            newContext.Destination = serviceName;

            if (initialVisibilityDelay != null)
            {
                newContext.MessageDelayedTime = initialVisibilityDelay;
                newContext.MessageFinalWakeUpTimeStamp = DateTime.UtcNow + initialVisibilityDelay;
            }
            else
            {
                newContext.MessageDelayedTime = null;
                newContext.MessageFinalWakeUpTimeStamp = null;
            }
            

            await sender.SendMessageAsync(input, serviceName, newContext).ConfigureAwait(false);
           
        }

        public async Task PublishAsync<T>(T input, string topic, TimeSpan? initialVisibilityDelay = null) where T : class
        {
            var newContext = new AFBusMessageContext();

            newContext.MessageID = Guid.NewGuid();
            newContext.TransactionID = Context.TransactionID ?? Guid.NewGuid();
            newContext.BodyType = typeof(T).AssemblyQualifiedName;
            newContext.BodyInFile = false;
            newContext.Destination = topic;

            if (initialVisibilityDelay != null)
            {
                newContext.MessageDelayedTime = initialVisibilityDelay;
                newContext.MessageFinalWakeUpTimeStamp = DateTime.UtcNow + initialVisibilityDelay;
            }
            else
            {
                newContext.MessageDelayedTime = null;
                newContext.MessageFinalWakeUpTimeStamp = null;
            }


            await publisher.PublishEventsAsync(input, topic, newContext).ConfigureAwait(false);
        }
    }
}
