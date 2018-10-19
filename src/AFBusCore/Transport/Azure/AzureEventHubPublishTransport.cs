using Microsoft.Azure.EventHubs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    public class AzureEventHubPublishTransport : IPublishEvents
    {
        private const int MAX_MESSAGE_SIZE = 256000;
        private const string CONTAINER_NAME = "bigmessages";
        ISerializeMessages serializer;

        static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(SettingsUtil.GetSettings<string>(SETTINGS.AZURE_STORAGE));
        static CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

        public AzureEventHubPublishTransport(ISerializeMessages serializer)
        {
            this.serializer = serializer;
        }

        public async Task PublishEventsAsync<T>(T message, string topicName, AFBusMessageContext messageContext) where T : class
        {
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(SettingsUtil.GetSettings<string>(SETTINGS.AZURE_EVENTHUB))
            {
                EntityPath = topicName
            };

            var eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(SettingsUtil.GetSettings<string>(SETTINGS.AZURE_STORAGE));

            try
            {
                var messageAsString = serializer.Serialize(message);

                var messageWithEnvelope = new AFBusMessageEnvelope()
                {
                    Context = messageContext,
                    Body = messageAsString
                };

                messageContext.Destination = topicName;


                var finalMessage = serializer.Serialize(messageWithEnvelope);

                //if the message is bigger than the limit put the body in the blob storage
                if ((finalMessage.Length * sizeof(Char)) > MAX_MESSAGE_SIZE)
                {
                    var fileName = Guid.NewGuid().ToString("N").ToLower() + ".afbus";
                    messageWithEnvelope.Context.BodyInFile = true;
                                        

                    // Create a container 
                    var cloudBlobContainer = cloudBlobClient.GetContainerReference(CONTAINER_NAME.ToLower());
                    await cloudBlobContainer.CreateIfNotExistsAsync().ConfigureAwait(false);

                    CloudBlockBlob blockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
                    await blockBlob.UploadTextAsync(messageWithEnvelope.Body);

                    messageWithEnvelope.Body = fileName;

                    finalMessage = serializer.Serialize(messageWithEnvelope);
                }



                await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(finalMessage)));
            }
            finally
            {
                await eventHubClient.CloseAsync();
            }
        }

        public async Task<string> ReadMessageBodyFromFileAsync(string fileName)
        {            
            // Create a container 
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(CONTAINER_NAME.ToLower());
            await cloudBlobContainer.CreateIfNotExistsAsync().ConfigureAwait(false);

            CloudBlockBlob blockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
            return await blockBlob.DownloadTextAsync();


        }

        public async Task DeleteFileWithMessageBodyAsync(string fileName)
        {

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
