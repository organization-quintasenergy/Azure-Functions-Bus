using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace AFBus.Tests
{
    internal class QueueReader
    {
        internal static async Task<string> ReadFromQueue(string serviceName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Properties.Settings.Default.StorageConnectionString);

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            CloudQueue queue = queueClient.GetQueueReference(serviceName.ToLower());
            queue.CreateIfNotExists();

            var message = await queue.GetMessageAsync().ConfigureAwait(false);

            return message.AsString;
        }
    }
}
