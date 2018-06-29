using AFBus;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AFBusCore.Sagas
{
    public interface IHandleEventStartingSaga<MessageType>
    {
        /// <summary>
        /// Handles an event that creates a Saga
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="message"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        Task HandleEventAsync(IBus bus, MessageType message, TraceWriter log);
    }
}
