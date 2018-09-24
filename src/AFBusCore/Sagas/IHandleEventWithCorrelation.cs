using AFBus;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    public interface IHandleEventWithCorrelation<MessageType>
    {
        /// <summary>
        /// Handles an event 
        /// </summary>
        /// <param name="bus"></param>
        /// <param name="message"></param>
        /// <param name="Log"></param>
        Task HandleEventAsync(IBus bus, MessageType message, ILogger log);

        /// <summary>
        /// Defines how a message correlates to a saga instance
        /// </summary>
        Task<List<SagaData>> LookForInstanceAsync(MessageType message);
    }
}
