using AFBus;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UIExample
{
    public interface IAFBusService : IHostedService
    {

    }

    public class AFBusService : IAFBusService
    {
        public static HandlersContainer handlerContainer = new HandlersContainer();

        public static string UI_SERVICE_NAME = "uiexample";

        IHubContext<Events> hubcontext;

        static AFBusService()
        {
            
        }

        public AFBusService(IHubContext<Events> hubcontext)
        {
            this.hubcontext = hubcontext;

            HandlersContainer.AddDependencyWithInstance<IHubContext<Events>>(hubcontext);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(SettingsUtil.GetSettings<string>(SETTINGS.AZURE_STORAGE));

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            CloudQueue queue = queueClient.GetQueueReference(UI_SERVICE_NAME.ToLower());
            queue.CreateIfNotExists();


            /*while (true)
            {
                // Get the next message
                CloudQueueMessage retrievedMessage = queue.GetMessage();

                await handlerContainer.HandleAsync(retrievedMessage.AsString, null);

                //Process the message in less than 30 seconds, and then delete the message
                queue.DeleteMessage(retrievedMessage);
            }*/

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
