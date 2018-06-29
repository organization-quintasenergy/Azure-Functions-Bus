using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    public class AzureServiceBusPublishTransport : IPublishEvents
    {
        private const int MAX_MESSAGE_SIZE = 256000;
        private const string CONTAINER_NAME = "bigmessages";
        ISerializeMessages serializer;

        public AzureServiceBusPublishTransport(ISerializeMessages serializer)
        {
            this.serializer = serializer;
        }

        public async Task PublishEventsAsync<T>(T message, string topicName, AFBusMessageContext messageContext) where T : class
        {
            
            var sender = new MessageSender(SettingsUtil.GetSettings<string>(SETTINGS.AZURE_SERVICEBUS), topicName.ToLower());
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(SettingsUtil.GetSettings<string>(SETTINGS.AZURE_STORAGE));


            var messageAsString = serializer.Serialize(message);

            var messageWithEnvelope = new AFBusMessageEnvelope()
            {
                Context = messageContext,
                Body = messageAsString
            };

            messageContext.Destination = topicName;

            TimeSpan? initialVisibilityDelay = null;

            if (messageContext.MessageDelayedTime != null && messageContext.MessageDelayedTime >= MaxDelay())
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
            if ((finalMessage.Length * sizeof(Char)) > MAX_MESSAGE_SIZE)
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


            var finalSBMessage = new Message(Encoding.UTF8.GetBytes(finalMessage))
            {
                ContentType = "application/json",
                Label = topicName,
                MessageId = messageContext.MessageID.ToString(),
                TimeToLive = TimeSpan.FromDays(10)               
            };

            if (messageContext.MessageDelayedTime.HasValue)
                finalSBMessage.ScheduledEnqueueTimeUtc = DateTime.UtcNow + messageContext.MessageDelayedTime.Value;


            await sender.SendAsync(finalSBMessage).ConfigureAwait(false); 
            
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

        public virtual TimeSpan MaxDelay()
        {
            return new TimeSpan(7, 0, 0, 0);
        }
    }
}
