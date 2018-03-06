using System;
using System.Threading.Tasks;

namespace AFBus
{
    public interface IHandlersContainer 
    {
        
        Task InvokeAsync<T>(T message, ITraceWriter log) where T : class;

        Task InvokeAsync(string serializedMessage, ITraceWriter log);

    }
}
