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
    public class Bus : IBus
    {
        ISerializeMessages serializer; 
        ISendMessages sender;

        internal Bus(ISerializeMessages serializer, ISendMessages sender)
        {
            this.serializer = serializer;
            this.sender = sender;
        }
        /// <summary>
        /// Sends a message to a queue named like the service.
        /// </summary>
        public async Task SendAsync<T>(T input, string serviceName, TimeSpan? initialVisibilityDelay = null) where T : class
        {
            await sender.AddMessageAsync(input, serviceName, initialVisibilityDelay);
           
        }
    }
}
