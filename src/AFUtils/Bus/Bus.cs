using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace AFUtils
{
    public class Bus : IBus
    {
        public async Task SendAsync<T>(T input, string serviceName)
        {
            CloudStorageAccount storageAccount = null;// CloudStorageAccount.Parse(Properties.Settings.Default.StorageConnectionString);

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            CloudQueue queue = queueClient.GetQueueReference(serviceName);
            queue.CreateIfNotExists();
                       
            await queue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(input))).ConfigureAwait(false);
           
        }
    }
}
