using System;
using System.Threading.Tasks;

namespace AFBus
{
    public interface IFunctionContainer 
    {
        
        Task InvokeAsync<T>(T message, ITraceWriter log);

    }
}
