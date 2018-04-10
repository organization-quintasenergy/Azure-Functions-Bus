using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace AFBus
{
    class AzureStorageQueueSendTransport : ISendMessages
    {
        ISerializeMessages serializer;

        private static HashSet<string> createdQueues = new HashSet<string>();

        public AzureStorageQueueSendTransport(ISerializeMessages serializer)
        {
            this.serializer = serializer;
        }

        public async Task SendMessageAsync<T>(T message, string serviceName, AFBusMessageContext messageContext) where T : class
        {
            serviceName = serviceName.ToLower();

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(SettingsUtil.GetSettings<string>(SETTINGS.AZURE_STORAGE));

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            CloudQueue queue = queueClient.GetQueueReference(serviceName);

            if (!createdQueues.Contains(serviceName) && queue.CreateIfNotExists())
            {                
                createdQueues.Add(serviceName);
            }

            
            var messageAsString = serializer.Serialize(message);

            var messageWithEnvelope = new AFBusMessageEnvelope()
            {
                Context = messageContext,
                Body = messageAsString
            };

            TimeSpan? initialVisibilityDelay = null;

            if (messageContext.DelayedFor != null && initialVisibilityDelay > MaxDelayInMinutes())
            {
                initialVisibilityDelay = MaxDelayInMinutes();
            }

            await queue.AddMessageAsync(new CloudQueueMessage(serializer.Serialize(messageWithEnvelope)), null, initialVisibilityDelay, null, null).ConfigureAwait(false);
        }

        public TimeSpan MaxDelayInMinutes()
        {
            return new TimeSpan(7,0, 0, 0);
        }
    }
}
