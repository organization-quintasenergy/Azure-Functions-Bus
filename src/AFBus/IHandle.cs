using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace AFBus
{

    public interface IHandle<MessageType> 
    {
        /// <summary>
        /// Handles a message in a stateless way.
        /// </summary>
        Task HandleAsync(IBus bus, MessageType message, TraceWriter Log);
           
    }
}
