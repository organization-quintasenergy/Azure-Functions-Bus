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
        public static async Task SendAsync<T>(T input, string serviceName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Properties.Settings.Default.StorageConnectionString);

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            CloudQueue queue = queueClient.GetQueueReference(serviceName.ToLower());
            queue.CreateIfNotExists();

            await queue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(input, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full
            }))).ConfigureAwait(false);

        }
    }
}
