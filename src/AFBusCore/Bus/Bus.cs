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

        internal Bus(ISerializeMessages serializer, ISendMessages sender)
        {
            this.serializer = serializer;
            this.sender = sender;
        }

        ISerializeMessages IBus.Serializer => this.serializer;

        public AFBusMessageContext Context { get; set; }

        /// <summary>
        /// Sends a message to a queue named like the service.
        /// </summary>
        public async Task SendAsync<T>(T input, string serviceName, TimeSpan? initialVisibilityDelay = null) where T : class
        {           

            Context.MessageID = Guid.NewGuid();
            Context.TransactionID = Context.TransactionID ?? Guid.NewGuid();
            Context.BodyType = typeof(T).AssemblyQualifiedName;
            Context.BodyInFile = false;

            if (initialVisibilityDelay != null)
            {                
                Context.MessageDelayedTime = initialVisibilityDelay;
                Context.MessageFinalWakeUpTimeStamp = DateTime.UtcNow + initialVisibilityDelay;
            }
            else
            {
                Context.MessageDelayedTime = null;
                Context.MessageFinalWakeUpTimeStamp = null;
            }
            

            await sender.SendMessageAsync(input, serviceName, Context).ConfigureAwait(false);
           
        }
    }
}
