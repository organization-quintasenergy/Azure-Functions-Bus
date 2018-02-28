using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace AFUtils
{
    public interface IFunction<MessageType> 
    {      

        Task InvokeAsync(MessageType input, ITraceWriter Log);
           
    }
}
