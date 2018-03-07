using System;
using System.Threading.Tasks;

namespace AFBus
{
    public interface IHandlersContainer 
    {
        
        Task HandleAsync<T>(T message, ITraceWriter log) where T : class;

        Task HandleAsync(string serializedMessage, ITraceWriter log);

    }
}
