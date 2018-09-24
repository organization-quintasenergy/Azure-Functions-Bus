using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFBus
{
    /// <summary>
    /// This interface defines that a message will start a saga. 
    /// </summary>
    public interface IHandleCommandStartingSaga<MessageType>
    {
        Task HandleCommandAsync(IBus bus, MessageType message, ILogger log);
    }
}
