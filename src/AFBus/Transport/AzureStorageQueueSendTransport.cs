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

            messageContext.Destination = serviceName;

            TimeSpan? initialVisibilityDelay = null;

            if (messageContext.DelayedTime != null && messageContext.DelayedTime >=  MaxDelay())
            {
                initialVisibilityDelay = MaxDelay();

                //substract the max delay from transport
                messageContext.DelayedTime = messageContext.DelayedTime - MaxDelay();
            }
            else if (messageContext.DelayedTime != null)
            {
                initialVisibilityDelay = messageContext.DelayedTime;

                messageContext.DelayedTime = null;
            }

            if (messageContext.DelayedTime != null && initialVisibilityDelay.Value.TotalMilliseconds < 0)
                initialVisibilityDelay = null;

            await queue.AddMessageAsync(new CloudQueueMessage(serializer.Serialize(messageWithEnvelope)), null, initialVisibilityDelay, null, null).ConfigureAwait(false);
        }

        public TimeSpan MaxDelay()
        {
            return new TimeSpan(7,0, 0, 0);
        }
    }
}
