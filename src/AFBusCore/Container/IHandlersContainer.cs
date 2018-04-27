using Microsoft.Azure.WebJobs.Host;
using System;
using System.Threading.Tasks;

namespace AFBus
{
    public interface IHandlersContainer 
    {
        
        Task HandleAsync<T>(T message, TraceWriter log) where T : class;

        Task HandleAsync(string serializedMessage, TraceWriter log);

    }
}
