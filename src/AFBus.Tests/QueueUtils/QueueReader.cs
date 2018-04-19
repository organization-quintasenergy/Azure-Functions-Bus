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
        internal static async Task<string> ReadOneMessageFromQueue(string serviceName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(SettingsUtil.GetSettings<string>(SETTINGS.AZURE_STORAGE));

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            CloudQueue queue = queueClient.GetQueueReference(serviceName.ToLower());
            queue.CreateIfNotExists();

            var message = await queue.GetMessageAsync().ConfigureAwait(false);
            
            if (message != null)
            {
                queue.DeleteMessage(message);
                return message.AsString;
            }
            else
                return null;
            
        }

        internal static async Task<IEnumerable<string>> ReadEveryMessageFromQueue(string serviceName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(SettingsUtil.GetSettings<string>(SETTINGS.AZURE_STORAGE));

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            CloudQueue queue = queueClient.GetQueueReference(serviceName.ToLower());
            queue.CreateIfNotExists();

            var messages = await queue.GetMessagesAsync(30);
                        
            if (messages != null)
                return messages.Select(m=> m.AsString);
            else
                return null;

        }

        internal static IEnumerable<string> ReadFromQueueToYield(string serviceName)
        {
            IEnumerable<string> result;

            while ((result = ReadEveryMessageFromQueue(serviceName).Result) != null && result.Count()>0)
            {
                foreach (var e in result)
                {
                    yield return e;
                }
                
            }

            
        }

        internal static async Task CleanQueue(string serviceName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(SettingsUtil.GetSettings<string>(SETTINGS.AZURE_STORAGE));

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            CloudQueue queue = queueClient.GetQueueReference(serviceName.ToLower());
            queue.CreateIfNotExists();

            await queue.ClearAsync().ConfigureAwait(false);            

        }
    }
}
