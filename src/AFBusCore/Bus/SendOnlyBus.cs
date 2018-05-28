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
    public class SendOnlyBus 
    {
        /// <summary>
        /// Sends a message to a queue named like the service.
        /// </summary>
        public static async Task SendAsync<T>(T input, string serviceName, TimeSpan? initialVisibilityDelay = null, ISerializeMessages serializer = null, ISendMessages sender = null) where T : class
        {
            serializer = serializer ?? new JSONSerializer();
            sender = sender ?? new AzureStorageQueueSendTransport(serializer);

            var context = new AFBusMessageContext
            {
                MessageID = Guid.NewGuid(),
                TransactionID = Guid.NewGuid(),
                BodyType = typeof(T).AssemblyQualifiedName
            };

            if (initialVisibilityDelay != null)
            {
                context.MessageDelayedTime = initialVisibilityDelay.Value ;
                context.MessageFinalWakeUpTimeStamp = DateTime.UtcNow + initialVisibilityDelay;
            }

            await sender.SendMessageAsync(input, serviceName, context).ConfigureAwait(false);

        }
    }
}
