using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace AFBus
{
    public interface IFunction<MessageType> 
    {      

        Task InvokeAsync(MessageType input, ITraceWriter Log);
           
    }
}
