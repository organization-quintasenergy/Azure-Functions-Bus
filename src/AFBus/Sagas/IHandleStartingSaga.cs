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
    public interface IHandleStartingSaga<MessageType>
    {
        Task HandleAsync(IBus bus, MessageType message, ITraceWriter Log);
    }
}
