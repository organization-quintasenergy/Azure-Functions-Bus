using AFBus;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using OrderSaga.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UIExample
{

    public class AFBusService : IHostedService
    {       

        public static string UI_SERVICE_NAME = "uiexample";

        public static HandlersContainer handlerContainer = new HandlersContainer(UI_SERVICE_NAME);

        IHubContext<Events> hubcontext;

        private Task executingTask;
        private CancellationTokenSource cts;

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

            // Create a linked token so we can trigger cancellation outside of this token's cancellation
            cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Store the task we're executing
            executingTask = ExecuteAsync(cts.Token);

            // If the task is completed then return it, otherwise it's running
            return executingTask.IsCompleted ? executingTask : Task.CompletedTask;

         
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // Stop called without start
            if (executingTask == null)
            {
                return;
            }

            // Signal cancellation to the executing method
            cts.Cancel();

            // Wait until the task completes or the stop token triggers
            await Task.WhenAny(executingTask, Task.Delay(-1, cancellationToken));

            // Throw if cancellation triggered
            cancellationToken.ThrowIfCancellationRequested();
        }

        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(SettingsUtil.GetSettings<string>(SETTINGS.AZURE_STORAGE));

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            CloudQueue queue = queueClient.GetQueueReference(UI_SERVICE_NAME.ToLower());
            await queue.CreateIfNotExistsAsync();


            while (!cancellationToken.IsCancellationRequested)
            {               
                // Get the next message
                CloudQueueMessage retrievedMessage = await queue.GetMessageAsync();

                if (retrievedMessage != null)
                {
                    await handlerContainer.HandleAsync(retrievedMessage.AsString, null);

                    //Process the message in less than 30 seconds, and then delete the message
                    await queue.DeleteMessageAsync(retrievedMessage);
                }

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }

           
        }
    }
}
