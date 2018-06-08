using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;

namespace AFBus
{
    class AzureStorageQueueSendTransport : ISendMessages
    {
        private const int MAX_MESSAGE_SIZE = 65536;
        private const string CONTAINER_NAME = "bigmessages";
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

            if (!createdQueues.Contains(serviceName))
            {
                await queue.CreateIfNotExistsAsync();
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

            if (messageContext.MessageDelayedTime != null && messageContext.MessageDelayedTime >=  MaxDelay())
            {
                initialVisibilityDelay = MaxDelay();
               
                messageContext.MessageDelayedTime = MaxDelay();
            }
            else if (messageContext.MessageDelayedTime != null)
            {
                initialVisibilityDelay = messageContext.MessageDelayedTime;
                
            }

            if (messageContext.MessageDelayedTime != null && initialVisibilityDelay.Value < TimeSpan.Zero)
            {
                initialVisibilityDelay = null;

                messageContext.MessageDelayedTime = null;
            }

            var finalMessage = serializer.Serialize(messageWithEnvelope);

            //if the message is bigger than the limit put the body in the blob storage
            if((finalMessage.Length * sizeof(Char))> MAX_MESSAGE_SIZE)
            {
                var fileName = Guid.NewGuid().ToString("N").ToLower() + ".afbus";
                messageWithEnvelope.Context.BodyInFile = true;

                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

                // Create a container 
                var cloudBlobContainer = cloudBlobClient.GetContainerReference(CONTAINER_NAME.ToLower());
                await cloudBlobContainer.CreateIfNotExistsAsync().ConfigureAwait(false);

                CloudBlockBlob blockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
                await blockBlob.UploadTextAsync(messageWithEnvelope.Body);

                messageWithEnvelope.Body = fileName;

                finalMessage = serializer.Serialize(messageWithEnvelope);
            }

            await queue.AddMessageAsync(new CloudQueueMessage(serializer.Serialize(messageWithEnvelope)), null, initialVisibilityDelay, null, null).ConfigureAwait(false);
        }

        public virtual TimeSpan MaxDelay()
        {
            return new TimeSpan(7, 0, 0, 0);
        }

        public async Task<string> ReadMessageBodyFromFileAsync(string fileName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(SettingsUtil.GetSettings<string>(SETTINGS.AZURE_STORAGE));
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

            // Create a container 
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(CONTAINER_NAME.ToLower());
            await cloudBlobContainer.CreateIfNotExistsAsync().ConfigureAwait(false);

            CloudBlockBlob blockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
            return await blockBlob.DownloadTextAsync();

           
        }

        public async Task DeleteFileWithMessageBodyAsync(string fileName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(SettingsUtil.GetSettings<string>(SETTINGS.AZURE_STORAGE));
            CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

            // Create a container 
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(CONTAINER_NAME.ToLower());
            await cloudBlobContainer.CreateIfNotExistsAsync().ConfigureAwait(false);

            CloudBlockBlob blockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
            await blockBlob.DeleteIfExistsAsync();
        }

        public int MaxMessageSize()
        {
            return MAX_MESSAGE_SIZE;
        }
    }
}
