using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace AFBus
{
    public interface IFunction<MessageType> 
    {      

        Task InvokeAsync(IBus bus, MessageType input, ITraceWriter Log);
           
    }
}
